#include "pch.h"
#include "perf_dll.h"
#include <string>
#include "stdafx.h"
#include <tchar.h>
#include <windows.h>	// Standard windows header
#include <winperf.h>	// Performance monitor definations
#include <stdio.h>		// printf() and other I/O stuff
#include <malloc.h>		// memory allocation definations.
#include <fstream>
#include <iomanip>
#include <sstream>
#include <list>
#include <strsafe.h>
#include <cmath>

#define _UNICODE
#define MAX_INSTANCE_NAME_LEN 255
#define BUFFERSIZE 2048
#define INCREMENT 1024

#pragma comment(lib, "advapi32.lib")

using namespace std;

extern "C" __declspec(dllexport) void GetDataBlockPointer(PPERF_DATA_BLOCK* p) {
	PPERF_DATA_BLOCK PerfDataBlock = NULL;
	DWORD BufferSize = BUFFERSIZE;
	PerfDataBlock = (PPERF_DATA_BLOCK)malloc(BufferSize);

	while (RegQueryValueEx(
		HKEY_PERFORMANCE_DATA,
		L"Global",
		NULL,
		NULL,
		(LPBYTE)PerfDataBlock,
		&BufferSize) == ERROR_MORE_DATA)
	{	// The buffer is too small, so expand it! .
		BufferSize += INCREMENT;
		PerfDataBlock = (PPERF_DATA_BLOCK)realloc(PerfDataBlock, BufferSize);
	}
	*p = PerfDataBlock;
}

extern "C" __declspec(dllexport) void GetDataBlockInfo(
	PPERF_DATA_BLOCK p,
	LONG* DefaultObject,
	DWORD* NumObjectTypes,
	LONGLONG* PerfFreq,
	LONGLONG* PerfTime,
	LONGLONG* PerfTime100nSec,
	DWORD* Revision,
	char* Signature,
	char* SystemName,
	char* SystemTime,
	DWORD* TotalByteLength,
	DWORD* Version) 
{
	*DefaultObject = p->DefaultObject;
	*NumObjectTypes = p->NumObjectTypes;
	*PerfFreq = p->PerfFreq.QuadPart;
	*PerfTime = p->PerfTime.QuadPart;
	*PerfTime100nSec = p->PerfTime100nSec.QuadPart;
	*Revision = p->Revision;
	*TotalByteLength = p->TotalByteLength;
	*Version = p->Version;

	stringstream ss;

	char* name = (char*)((PBYTE)p + p->SystemNameOffset);
	for (int i = 0; i < p->SystemNameLength; i = i +2) 
		ss << name + i;

	strcpy(SystemName, ss.str().c_str());
	ss.str("");

	ss << char(p->Signature[0]) << char(p->Signature[1]) << char(p->Signature[2]) << char(p->Signature[3]);
	strcpy(Signature, ss.str().c_str());
	ss.str("");

	SYSTEMTIME st = p->SystemTime;
	switch (st.wDayOfWeek) {
	case 0:
		ss << "Sunday";
		break;
	case 1:
		ss << "Monday";
		break;
	case 2:
		ss << "Tuesday";
		break;
	case 3:
		ss << "Wednesday";
		break;
	case 4:
		ss << "Thursday";
		break;
	case 5:
		ss << "Friday";
		break;
	case 6:
		ss << "Saturday";
		break;
	}
	ss << " " << st.wDay << "." << st.wMonth << "." << st.wYear << " "  
		<< st.wHour << ":" << st.wMinute << ":"  << st.wSecond << ":" << st.wMilliseconds;
	strcpy(SystemTime, ss.str().c_str());
}

extern "C" __declspec(dllexport) void GetFirstObjectTypePointer(PPERF_DATA_BLOCK p, PPERF_OBJECT_TYPE* pp) {
	*pp = (PPERF_OBJECT_TYPE)((PBYTE)p + p->HeaderLength);
}

extern "C" __declspec(dllexport) void GetNextObjectTypePointer(PPERF_OBJECT_TYPE p, PPERF_OBJECT_TYPE* pp) {
	*pp = (PPERF_OBJECT_TYPE)((PBYTE)p + p->TotalByteLength);
}

extern "C" __declspec(dllexport) void GetPerfObjectTypeInfo(
	PPERF_OBJECT_TYPE p, 
	DWORD* ObjectNameTitleIndex,
	DWORD* TotalByteLength,
	DWORD* ObjectHelpTitleIndex,
	DWORD* DetailLevel,
	DWORD* NumCounters,
	LONG* DefaultCounter,
	LONG* NumInstances,
	DWORD* CodePage,
	LONGLONG* PerfTime,
	LONGLONG* PerfFreq) 
{
	*ObjectNameTitleIndex =		p->ObjectNameTitleIndex;
	*TotalByteLength =			p->TotalByteLength;
	*ObjectHelpTitleIndex =		p->ObjectHelpTitleIndex;
	*DetailLevel =				p->DetailLevel;
	*NumCounters =				p->NumCounters;
	*DefaultCounter =			p->DefaultCounter;
	p->NumInstances > 0 ? *NumInstances = p->NumInstances : *NumInstances = 0;
	*CodePage =	p->CodePage;
	*PerfTime =	p->PerfTime.QuadPart;
	*PerfFreq =	p->PerfFreq.QuadPart;
}

extern "C" __declspec(dllexport) void GetCounterDefinitionPointer(PPERF_OBJECT_TYPE p, PPERF_COUNTER_DEFINITION* pp) {
	*pp = (PPERF_COUNTER_DEFINITION)((PBYTE)p + p->HeaderLength);
}

extern "C" __declspec(dllexport) void GetCounterBlock_InstPointer(PPERF_INSTANCE_DEFINITION p, PPERF_COUNTER_BLOCK* pp) {
	*pp = (PPERF_COUNTER_BLOCK)((PBYTE)p + p->ByteLength);
}

extern "C" __declspec(dllexport) void GetCounterBlock_ObjPointer(PPERF_OBJECT_TYPE p, PPERF_COUNTER_BLOCK * pp) {
	*pp = (PPERF_COUNTER_BLOCK)((PBYTE)p +	p->DefinitionLength);
}

extern "C" __declspec(dllexport) void GetFirstInstancePointer(PPERF_OBJECT_TYPE p, PPERF_INSTANCE_DEFINITION* pp) {
	PPERF_INSTANCE_DEFINITION PerfInstanceDefinition = (PPERF_INSTANCE_DEFINITION)((PBYTE)p + p->DefinitionLength);
	*pp = PerfInstanceDefinition;
}

BOOL ConvertNameToUnicode(UINT CodePage, LPCSTR pNameToConvert, DWORD dwNameToConvertLen, LPWSTR pConvertedName)
{
	BOOL fSuccess = FALSE;
	int CharsConverted = 0;
	DWORD dwLength = 0;

	// dwNameToConvertLen is in bytes, so convert MAX_INSTANCE_NAME_LEN to bytes.
	dwLength = (MAX_INSTANCE_NAME_LEN * sizeof(WCHAR) < (dwNameToConvertLen)) ? MAX_INSTANCE_NAME_LEN * sizeof(WCHAR) : dwNameToConvertLen;

	CharsConverted = MultiByteToWideChar((UINT)CodePage, 0, pNameToConvert, dwLength, pConvertedName, MAX_INSTANCE_NAME_LEN);
	if (CharsConverted)
	{
		pConvertedName[dwLength] = '\0';
		fSuccess = TRUE;
	}
	else
	{
		// If the specified code page didn't work, try one more time, assuming that the input string is Unicode.
		dwLength = (MAX_INSTANCE_NAME_LEN < (dwNameToConvertLen / 2)) ? MAX_INSTANCE_NAME_LEN : dwNameToConvertLen / 2;
		if (SUCCEEDED(StringCchCopyN(pConvertedName, MAX_INSTANCE_NAME_LEN + 1, (LPWSTR)pNameToConvert, dwLength)))
		{
			pConvertedName[dwLength] = '\0';
			fSuccess = TRUE;
		}
	}
	return fSuccess;
}

extern "C" __declspec(dllexport) void GetInstanceInfo(
	PPERF_INSTANCE_DEFINITION p,
	DWORD CodePage,
	DWORD* ByteLength,
	DWORD* ParentObjectTitleIndex, 
	DWORD* ParentObjectInstance, 
	WCHAR* Name)
{
	DWORD dwLength = 0;
	WCHAR wszInstanceName[MAX_INSTANCE_NAME_LEN + 1];
	BOOL fSuccess = TRUE;
	*ByteLength = p->ByteLength;
	*ParentObjectTitleIndex = p->ParentObjectTitleIndex;
	*ParentObjectInstance = p->ParentObjectInstance;
		
	if (CodePage == 0)  // Instance name is a Unicode string
	{
		// PERF_INSTANCE_DEFINITION->NameLength is in bytes, so convert to characters.
		dwLength = (MAX_INSTANCE_NAME_LEN < (p->NameLength / 2)) ? MAX_INSTANCE_NAME_LEN : p->NameLength / 2;
		StringCchCopyN(wszInstanceName, MAX_INSTANCE_NAME_LEN + 1, (LPWSTR)(((LPBYTE)p) + p->NameOffset), dwLength);
		wszInstanceName[dwLength] = '\0';
	}
	else  // Convert the multi-byte instance name to Unicode
	{
		fSuccess = ConvertNameToUnicode(CodePage,
			(LPCSTR)(((LPBYTE)p) + p->NameOffset),  // Points to string
			p->NameLength,
			wszInstanceName);

		if (FALSE == fSuccess)
		{
			wprintf(L"ConvertNameToUnicode for instance failed.\n");
			goto cleanup;
		}
	}
	
	StringCchPrintf(Name, MAX_INSTANCE_NAME_LEN + 1, L"%s", wszInstanceName);
cleanup:
	return;
}



extern "C" __declspec(dllexport) void GetNextInstancePointer(PPERF_COUNTER_BLOCK p,PPERF_INSTANCE_DEFINITION* pp) {
	*pp = (PPERF_INSTANCE_DEFINITION)((PBYTE)p + p->ByteLength);
}

extern "C" __declspec(dllexport) void GetCounterInfo(
	PPERF_COUNTER_DEFINITION p,
	DWORD* ByteLength,
	DWORD* CounterHelpTitleIndex,
	DWORD* CounterNameTitleIndex,
	DWORD* CounterSize,
	DWORD* CounterType,
	LONG* DefaultScale,
	DWORD* DetailLevel)
{
	*ByteLength = p->ByteLength;
	*CounterHelpTitleIndex = p->CounterHelpTitleIndex;
	*CounterNameTitleIndex = p->CounterNameTitleIndex;
	*CounterSize = p->CounterSize;
	*CounterType = p->CounterType;
	*DefaultScale = p->DefaultScale;
	*DetailLevel = p->DetailLevel;
}


extern "C" __declspec(dllexport) void GetCounterValue(
	PPERF_DATA_BLOCK g_pPerfDataHead,
	PPERF_OBJECT_TYPE pObject,
	PPERF_COUNTER_DEFINITION pCounter, 
	PPERF_COUNTER_BLOCK pCounterDataBlock, 
	ULONGLONG* Data,
	LONGLONG* Time,
	DWORD* MultiCounterData,
	LONGLONG* Frequency) 
{
	PVOID pData = NULL;
	UNALIGNED ULONGLONG* pullData = NULL;
	PERF_COUNTER_DEFINITION* pBaseCounter = NULL;
	BOOL fSuccess = TRUE;

	//Point to the raw counter data.
	pData = (PVOID)((LPBYTE)pCounterDataBlock + pCounter->CounterOffset);

	switch (pCounter->CounterType) 
	{
	case PERF_COUNTER_COUNTER:
	case PERF_COUNTER_QUEUELEN_TYPE:
	case PERF_SAMPLE_COUNTER:
		*Data = (ULONGLONG)(*(DWORD*)pData);
		*Time = ((PERF_DATA_BLOCK*)g_pPerfDataHead)->PerfTime.QuadPart;
		if (PERF_COUNTER_COUNTER == pCounter->CounterType || PERF_SAMPLE_COUNTER == pCounter->CounterType)
		{
			*Frequency = ((PERF_DATA_BLOCK*)g_pPerfDataHead)->PerfFreq.QuadPart;
		}
		break;

	case PERF_OBJ_TIME_TIMER:
		*Data = (ULONGLONG)(*(DWORD*)pData);
		*Time = pObject->PerfTime.QuadPart;
		break;

	case PERF_COUNTER_100NS_QUEUELEN_TYPE:
		*Data = *(UNALIGNED ULONGLONG*)pData;
		*Time = ((PERF_DATA_BLOCK*)g_pPerfDataHead)->PerfTime100nSec.QuadPart;
		break;

	case PERF_COUNTER_OBJ_TIME_QUEUELEN_TYPE:
		*Data = *(UNALIGNED ULONGLONG*)pData;
		*Time = pObject->PerfTime.QuadPart;
		break;

	case PERF_COUNTER_TIMER:
	case PERF_COUNTER_TIMER_INV:
	case PERF_COUNTER_BULK_COUNT:
	case PERF_COUNTER_LARGE_QUEUELEN_TYPE:
		pullData = (UNALIGNED ULONGLONG*)pData;
		*Data = *pullData;
		*Time = ((PERF_DATA_BLOCK*)g_pPerfDataHead)->PerfTime.QuadPart;
		if (pCounter->CounterType == PERF_COUNTER_BULK_COUNT)
		{
			*Frequency = ((PERF_DATA_BLOCK*)g_pPerfDataHead)->PerfFreq.QuadPart;
		}
		break;

	case PERF_COUNTER_MULTI_TIMER:
	case PERF_COUNTER_MULTI_TIMER_INV:
		pullData = (UNALIGNED ULONGLONG*)pData;
		*Data = *pullData;
		*Frequency = ((PERF_DATA_BLOCK*)g_pPerfDataHead)->PerfFreq.QuadPart;
		*Time = ((PERF_DATA_BLOCK*)g_pPerfDataHead)->PerfTime.QuadPart;

		//These counter types have a second counter value that is adjacent to
		//this counter value in the counter data block. The value is needed for
		//the calculation.
		if ((pCounter->CounterType & PERF_MULTI_COUNTER) == PERF_MULTI_COUNTER)
		{
			++pullData;
			*MultiCounterData = *(DWORD*)pullData;
		}
		break;

	//These counters do not use any time reference.
	case PERF_COUNTER_RAWCOUNT:
	case PERF_COUNTER_RAWCOUNT_HEX:
	case PERF_COUNTER_DELTA:
		*Data = (ULONGLONG)(*(DWORD*)pData);
		*Time = 0;
		break;

	case PERF_COUNTER_LARGE_RAWCOUNT:
	case PERF_COUNTER_LARGE_RAWCOUNT_HEX:
	case PERF_COUNTER_LARGE_DELTA:
		*Data = *(UNALIGNED ULONGLONG*)pData;
		*Time = 0;
		break;

	//These counters use the 100ns time base in their calculation.
	case PERF_100NSEC_TIMER:
	case PERF_100NSEC_TIMER_INV:
	case PERF_100NSEC_MULTI_TIMER:
	case PERF_100NSEC_MULTI_TIMER_INV:
		pullData = (UNALIGNED ULONGLONG*)pData;
		*Data = *pullData;
		*Time = ((PERF_DATA_BLOCK*)g_pPerfDataHead)->PerfTime100nSec.QuadPart;

		//These counter types have a second counter value that is adjacent to
		//this counter value in the counter data block. The value is needed for
		//the calculation.
		if ((pCounter->CounterType & PERF_MULTI_COUNTER) == PERF_MULTI_COUNTER)
		{
			++pullData;
			*MultiCounterData = *(DWORD*)pullData;
		}
		break;

	//These counters use two data points, this value and one from this counter's
	//base counter. The base counter should be the next counter in the object's 
	//list of counters.
	case PERF_SAMPLE_FRACTION:
	case PERF_RAW_FRACTION:
		*Data = (ULONGLONG)(*(DWORD*)pData);
		pBaseCounter = pCounter + 1;  //Get base counter
		if ((pBaseCounter->CounterType & PERF_COUNTER_BASE) == PERF_COUNTER_BASE)
		{
			pData = (PVOID)((LPBYTE)pCounterDataBlock + pBaseCounter->CounterOffset);
			*Time = (LONGLONG)(*(DWORD*)pData);
		}
		else
		{
			fSuccess = FALSE;
		}
		break;

	case PERF_LARGE_RAW_FRACTION:
		*Data = *(UNALIGNED ULONGLONG*)pData;
		pBaseCounter = pCounter + 1;
		if ((pBaseCounter->CounterType & PERF_COUNTER_BASE) == PERF_COUNTER_BASE)
		{
			pData = (PVOID)((LPBYTE)pCounterDataBlock + pBaseCounter->CounterOffset);
			*Time = *(LONGLONG*)pData;
		}
		else
		{
			fSuccess = FALSE;
		}
		break;

	case PERF_PRECISION_SYSTEM_TIMER:
	case PERF_PRECISION_100NS_TIMER:
	case PERF_PRECISION_OBJECT_TIMER:
		*Data = *(UNALIGNED ULONGLONG*)pData;
		pBaseCounter = pCounter + 1;
		if ((pBaseCounter->CounterType & PERF_COUNTER_BASE) == PERF_COUNTER_BASE)
		{
			pData = (PVOID)((LPBYTE)pCounterDataBlock + pBaseCounter->CounterOffset);
			*Time = *(LONGLONG*)pData;
		}
		else
		{
			fSuccess = FALSE;
		}
		break;

	case PERF_AVERAGE_TIMER:
	case PERF_AVERAGE_BULK:
		*Data = *(UNALIGNED ULONGLONG*)pData;
		pBaseCounter = pCounter + 1;
		if ((pBaseCounter->CounterType & PERF_COUNTER_BASE) == PERF_COUNTER_BASE)
		{
			pData = (PVOID)((LPBYTE)pCounterDataBlock + pBaseCounter->CounterOffset);
			*Time = *(DWORD*)pData;
		}
		else
		{
			fSuccess = FALSE;
		}

		if (pCounter->CounterType == PERF_AVERAGE_TIMER)
		{
			*Frequency = ((PERF_DATA_BLOCK*)g_pPerfDataHead)->PerfFreq.QuadPart;
		}
		break;

	//These are base counters and are used in calculations for other counters.
	//This case should never be entered.
	case PERF_SAMPLE_BASE:
	case PERF_AVERAGE_BASE:
	case PERF_COUNTER_MULTI_BASE:
	case PERF_RAW_BASE:
	case PERF_LARGE_RAW_BASE:
		*Data = 0;
		*Time = 0;
		break;

	case PERF_ELAPSED_TIME:
		*Data = *(UNALIGNED ULONGLONG*)pData;
		*Time = pObject->PerfTime.QuadPart;
		*Frequency = pObject->PerfFreq.QuadPart;
		break;

	//These counters are currently not supported.
	case PERF_COUNTER_TEXT:
	case PERF_COUNTER_NODATA:
	case PERF_COUNTER_HISTOGRAM_TYPE:
		*Data = 0;
		*Time = 0;
		break;

	//Encountered an unidentified counter.
	default:
		*Data = 0;
		*Time = 0;
		break;
	}
}



extern "C" __declspec(dllexport) void GetCalculatedValue(
	DWORD CounterType0,
	ULONGLONG Data0,
	LONGLONG Time0,
	DWORD MultiCounterData0,
	LONGLONG Frequency0,									
	DWORD CounterType1,
	ULONGLONG Data1,
	LONGLONG Time1,
	DWORD MultiCounterData1,
	LONGLONG Frequency1,
	wchar_t* FinalValue) 
{
	BOOL fSuccess = TRUE;
	ULONGLONG numerator = 0;
	LONGLONG denominator = 0;
	double doubleValue = 0;
	DWORD dwordValue = 0;
	wstringstream ss;
	wstring str;
	
	if (0 == CounterType1)
	{
		// Return error if the counter type requires two samples to calculate the value.
		switch (CounterType0)
		{
		default:
			if (PERF_DELTA_COUNTER != (CounterType0 & PERF_DELTA_COUNTER))
			{
				break;
			}
			__fallthrough;
		case PERF_AVERAGE_TIMER: // Special case.
		case PERF_AVERAGE_BULK:  // Special case.
			ss << L"The counter type requires two samples but only one sample was provided";
			fSuccess = FALSE;
			goto cleanup;
		}
	}
	else if (CounterType0 != CounterType1)
	{
		ss << L"The samples have inconsistent counter types";
		fSuccess = FALSE;
		goto cleanup;

	}
	
	switch (CounterType0)
	{
	case PERF_COUNTER_COUNTER:
	case PERF_SAMPLE_COUNTER:
	case PERF_COUNTER_BULK_COUNT:
		// (N1 - N0) / ((D1 - D0) / F)
		numerator = Data1 - Data0;
		denominator = Time1 - Time0;
		dwordValue = (DWORD)(numerator / ((double)denominator / Frequency1));
		ss << dwordValue;
		if (CounterType0 == PERF_SAMPLE_COUNTER)
			ss << L"";
		else 
			ss << L"/sec";
		break;

	case PERF_COUNTER_QUEUELEN_TYPE:
	case PERF_COUNTER_100NS_QUEUELEN_TYPE:
	case PERF_COUNTER_OBJ_TIME_QUEUELEN_TYPE:
	case PERF_COUNTER_LARGE_QUEUELEN_TYPE:
	case PERF_AVERAGE_BULK:  // normally not displayed
		// (N1 - N0) / (D1 - D0)
		numerator = Data1 - Data0;
		denominator = Time1 - Time0;
		doubleValue = (double)numerator / denominator;
		if (CounterType0 != PERF_AVERAGE_BULK)
			ss << doubleValue;
		break;

	case PERF_OBJ_TIME_TIMER:
	case PERF_COUNTER_TIMER:
	case PERF_100NSEC_TIMER:
	case PERF_PRECISION_SYSTEM_TIMER:
	case PERF_PRECISION_100NS_TIMER:
	case PERF_PRECISION_OBJECT_TIMER:
	case PERF_SAMPLE_FRACTION:
		// 100 * (N1 - N0) / (D1 - D0)
		numerator = (Data1 > Data0) ? (Data1 - Data0) : (Data0 - Data1);
		denominator = Time1 - Time0;
		doubleValue = (numerator < denominator) ? (double)(100 * numerator) / denominator : (double)(100 * denominator) / numerator;
		ss << doubleValue << L"%";
		break;

	case PERF_COUNTER_TIMER_INV:
		// 100 * (1 - ((N1 - N0) / (D1 - D0)))
		numerator = (Data1 > Data0) ? (Data1 - Data0) : (Data0 - Data1);
		denominator = Time1 - Time0;
		doubleValue = 100 * (1 - ((double)numerator / denominator));
		ss << doubleValue << L"%";
		break;

	case PERF_100NSEC_TIMER_INV:
		// 100 * (1- (N1 - N0) / (D1 - D0))
		numerator = (Data1 > Data0) ? (Data1 - Data0) : (Data0 - Data1);
		denominator = Time1 - Time0;
		doubleValue = 100 * (1 - (double)numerator / denominator);
		ss << doubleValue << L"%";
		break;

	case PERF_COUNTER_MULTI_TIMER:
		// 100 * ((N1 - N0) / ((D1 - D0) / TB)) / B1
		numerator = (Data1 > Data0) ? (Data1 - Data0) : (Data0 - Data1);
		denominator = Time1 - Time0;
		denominator /= Frequency1;
		doubleValue = 100 * ((double)numerator / denominator) / MultiCounterData1;
		ss << doubleValue << L"%";
		break;

	case PERF_100NSEC_MULTI_TIMER:
		// 100 * ((N1 - N0) / (D1 - D0)) / B1
		numerator = (Data1 > Data0) ? (Data1 - Data0) : (Data0 - Data1);
		denominator = Time1 - Time0;
		doubleValue = 100 * ((double)numerator / denominator) / MultiCounterData1;
		ss << doubleValue << L"%";
		break;

	case PERF_COUNTER_MULTI_TIMER_INV:
	case PERF_100NSEC_MULTI_TIMER_INV:
		// 100 * (B1 - ((N1 - N0) / (D1 - D0)))
		numerator = (Data1 > Data0) ? (Data1 - Data0) : (Data0 - Data1);
		denominator = Time1 - Time0;
		doubleValue = 100 * (MultiCounterData1 - ((double)numerator / denominator));
		ss << doubleValue << L"%";
		break;

	case PERF_COUNTER_RAWCOUNT:
	case PERF_COUNTER_LARGE_RAWCOUNT:
		// N as decimal
		ss << Data0;
		break;

	case PERF_COUNTER_RAWCOUNT_HEX:
	case PERF_COUNTER_LARGE_RAWCOUNT_HEX:
		// N as hexadecimal
		ss << Data0 << L" hex";
		break;

	case PERF_COUNTER_DELTA:
	case PERF_COUNTER_LARGE_DELTA:
		// N1 - N0
		ss << Data1 - Data0;
		break;

	case PERF_RAW_FRACTION:
	case PERF_LARGE_RAW_FRACTION:
		// 100 * N / B
		doubleValue = (double)100 * Data0 / Time0;
		ss << doubleValue << L"%";
		break;

	case PERF_AVERAGE_TIMER:
		// ((N1 - N0) / TB) / (B1 - B0)
		numerator = Data1 - Data0;
		denominator = Time1 - Time0;
		doubleValue = (double)numerator / Frequency1 / denominator;
		ss << doubleValue << L" seconds";
		break;

	case PERF_ELAPSED_TIME:
		// (D0 - N0) / F
		doubleValue = (double)(Time0 - Data0) / Frequency0;
		ss << doubleValue << L" seconds";
		break;

	case PERF_COUNTER_TEXT:
	case PERF_SAMPLE_BASE:
	case PERF_AVERAGE_BASE:
	case PERF_COUNTER_MULTI_BASE:
	case PERF_RAW_BASE:
	case PERF_COUNTER_NODATA:
	case PERF_PRECISION_TIMESTAMP:
		ss << L"Non-printing counter type";
		break;

	default:
		ss << L"Unrecognized counter type";
		fSuccess = FALSE;
		break;
	}
	cleanup:	
	wcscpy(FinalValue, ss.str().c_str());
}

extern "C" __declspec(dllexport) void GetNextCounterPointer(PPERF_COUNTER_DEFINITION p, PPERF_COUNTER_DEFINITION* pp) {
	*pp = (PPERF_COUNTER_DEFINITION)((PBYTE)p +	p->ByteLength);
}

extern "C" __declspec(dllexport) void GetCounterType(int numbertype, char* name, char* description) 
{
	string Name;
	string Description;
	switch (numbertype) 
	{
	case PERF_COUNTER_COUNTER:
		Name = "PERF_COUNTER_COUNTER";
		Description = "32-bit Counter.  Divide delta by delta time.  Display suffix: \"/sec\"";
		break;
	case PERF_COUNTER_TIMER:
		Name = "PERF_COUNTER_TIMER";
		Description = "64-bit Timer.  Divide delta by delta time.  Display suffix: \"%\"";
		break;
	case PERF_COUNTER_QUEUELEN_TYPE:
		Name = "PERF_COUNTER_QUEUELEN_TYPE";
		Description = "Queue Length Space-Time Product. Divide delta by delta time. No Display Suffix";
		break;
	case PERF_COUNTER_LARGE_QUEUELEN_TYPE:
		Name = "PERF_COUNTER_LARGE_QUEUELEN_TYPE";
		Description = "Queue Length Space-Time Product. Divide delta by delta time. No Display Suffix";
		break;
	case PERF_COUNTER_100NS_QUEUELEN_TYPE:
		Name = "PERF_COUNTER_100NS_QUEUELEN_TYPE";
		Description = "Queue Length Space-Time Product using 100 Ns timebase. Divide delta by delta time. No Display Suffix";
		break;
	case PERF_COUNTER_OBJ_TIME_QUEUELEN_TYPE:
		Name = "PERF_COUNTER_OBJ_TIME_QUEUELEN_TYPE";
		Description = "Queue Length Space-Time Product using Object specific timebase. Divide delta by delta time. No Display Suffix.";
		break;
	case PERF_COUNTER_BULK_COUNT:
		Name = "PERF_COUNTER_BULK_COUNT";
		Description = "64-bit Counter.  Divide delta by delta time. Display Suffix: \"/sec\"";
		break;
	case PERF_COUNTER_TEXT:
		Name = "PERF_COUNTER_TEXT";
		Description = "Indicates the counter is not a  counter but rather Unicode text Display as text.";
		break;
	case PERF_COUNTER_RAWCOUNT:
		Name = "PERF_COUNTER_RAWCOUNT";
		Description = "Indicates the data is a counter which should not be time averaged on display (such as an error counter on a serial line). Display as is.  No Display Suffix.";
		break;
	case PERF_COUNTER_LARGE_RAWCOUNT:
		Name = "PERF_COUNTER_LARGE_RAWCOUNT";
		Description = "Same as PERF_COUNTER_RAWCOUNT except its size is a large integer";
		break;
	case PERF_COUNTER_RAWCOUNT_HEX:
		Name = "PERF_COUNTER_RAWCOUNT_HEX";
		Description = "Special case for RAWCOUNT that want to be displayed in hex. Indicates the data is a counter  which should not be time averaged on display (such as an error counter on a serial line). Display as is.  No Display Suffix.";
		break;
	case PERF_COUNTER_LARGE_RAWCOUNT_HEX:
		Name = "PERF_COUNTER_LARGE_RAWCOUNT_HEX";
		Description = "Same as PERF_COUNTER_RAWCOUNT_HEX except its size is a large integer";
		break;
	case PERF_SAMPLE_FRACTION:
		Name = "PERF_SAMPLE_FRACTION";
		Description = "A count which is either 1 or 0 on each sampling interrupt (% busy). Divide delta by delta base. Display Suffix: \"%\"";
		break;
	case PERF_SAMPLE_COUNTER:
		Name = "PERF_SAMPLE_COUNTER";
		Description = "A count which is sampled on each sampling interrupt (queue length). Divide delta by delta time. No Display Suffix.";
		break;
	case PERF_COUNTER_NODATA:
		Name = "PERF_COUNTER_NODATA";
		Description = "A label: no data is associated with this counter (it has 0 length). Do not display.";
		break;
	case PERF_COUNTER_TIMER_INV:
		Name = "PERF_COUNTER_TIMER_INV";
		Description = "64-bit Timer inverse (e.g., idle is measured, but display busy %). Display 100 - delta divided by delta time.  Display suffix: \"%\"";
		break;
	case PERF_SAMPLE_BASE:
		Name = "PERF_SAMPLE_BASE";
		Description = "The divisor for a sample, used with the previous counter to form a sampled %. You must check for >0 before dividing by this! This counter will directly follow the numerator counter. It should not be displayed to the user.";
		break;
	case PERF_AVERAGE_TIMER:
		Name = "PERF_AVERAGE_TIMER";
		Description = "A timer which, when divided by an average base, produces a time in seconds which is the average time of some operation. This timer times total operations, and the base is the number of operations. Display Suffix: \"sec\"";
		break;
	case PERF_AVERAGE_BASE:
		Name = "PERF_AVERAGE_BASE";
		Description = "Used as the denominator in the computation of time or count averages. Must directly follow the numerator counter. Not displayed to the user.";
		break;
	case PERF_AVERAGE_BULK:
		Name = "PERF_AVERAGE_BULK";
		Description = "A bulk count which, when divided (typically) by the number of operations, gives (typically) the number of bytes per operation. No Display Suffix.";
		break;
	case PERF_OBJ_TIME_TIMER:
		Name = "PERF_OBJ_TIME_TIMER";
		Description = "64-bit Timer in object specific units. Display delta divided by delta time as returned in the object type header structure.  Display suffix: \"%\"";
		break;
	case PERF_100NSEC_TIMER:
		Name = "PERF_100NSEC_TIMER";
		Description = "64-bit Timer in 100 nsec units. Display delta divided by delta time. Display suffix: \"%\"";
		break;
	case PERF_100NSEC_TIMER_INV:
		Name = "PERF_100NSEC_TIMER_INV";
		Description = "64-bit Timer inverse (e.g., idle is measured, but display busy %). Display 100 - delta divided by delta time.  Display suffix: \"%\"";
		break;
	case PERF_COUNTER_MULTI_TIMER:
		Name = "PERF_COUNTER_MULTI_TIMER";
		Description = "64-bit Timer.  Divide delta by delta time.  Display suffix: \"%\". Timer for multiple instances, so result can exceed 100%.";
		break;
	case PERF_COUNTER_MULTI_TIMER_INV:
		Name = "PERF_COUNTER_MULTI_TIMER_INV";
		Description = "64-bit Timer inverse (e.g., idle is measured, but display busy %). Display 100 * _MULTI_BASE - delta divided by delta time. Display suffix: \"%\" Timer for multiple instances, so result can exceed 100%. Followed by a counter of type _MULTI_BASE.";
		break;
	case PERF_COUNTER_MULTI_BASE:
		Name = "PERF_COUNTER_MULTI_BASE";
		Description = "Number of instances to which the preceding _MULTI_..._INV counter applies. Used as a factor to get the percentage.";
		break;
	case PERF_100NSEC_MULTI_TIMER:
		Name = "PERF_100NSEC_MULTI_TIMER";
		Description = "64-bit Timer in 100 nsec units. Display delta divided by delta time. Display suffix: \"%\" Timer for multiple instances, so result can exceed 100%.";
		break;
	case PERF_100NSEC_MULTI_TIMER_INV:
		Name = "PERF_100NSEC_MULTI_TIMER_INV";
		Description = "64-bit Timer inverse (e.g., idle is measured, but display busy %). Display 100 * _MULTI_BASE - delta divided by delta time. Display suffix: \"%\" Timer for multiple instances, so result can exceed 100%. Followed by a counter of type _MULTI_BASE.";
		break;
	case PERF_RAW_FRACTION:
		Name = "PERF_RAW_FRACTION";
		Description = "Indicates the data is a fraction of the following counter  which should not be time averaged on display (such as free space over total space.) Display as is. Display the quotient as \"%\"";
		break;
	case PERF_LARGE_RAW_FRACTION:
		Name = "PERF_LARGE_RAW_FRACTION";
		Description = "Indicates the data is a fraction of the following counter  which should not be time averaged on display (such as free space over total space.) Display as is. Display the quotient as \"%\"";
		break;
	case PERF_RAW_BASE:
		Name = "PERF_RAW_BASE";
		Description = "Indicates the data is a base for the preceding counter which should not be time averaged on display (such as free space over total space.)";
		break;
	case PERF_LARGE_RAW_BASE:
		Name = "PERF_LARGE_RAW_BASE";
		Description = "Indicates the data is a base for the preceding counter which should not be time averaged on display (such as free space over total space.)";
		break;
	case PERF_ELAPSED_TIME:
		Name = "PERF_ELAPSED_TIME";
		Description = "The data collected in this counter is actually the start time of the item being measured. For display, this data is subtracted from the sample time to yield the elapsed time as the difference between the two. In the definition below, the PerfTime field of the Object contains the sample time as indicated by the PERF_OBJECT_TIMER bit and the difference is scaled by the PerfFreq of the Object to convert the time units into seconds.";
		break;
	case PERF_COUNTER_HISTOGRAM_TYPE:
		Name = "PERF_COUNTER_HISTOGRAM_TYPE";
		Description = "Counter type can be used with the preceding types to define a range of values to be displayed in a histogram.";
		break;
	case PERF_COUNTER_DELTA:
		Name = "PERF_COUNTER_DELTA";
		Description = "This counter is used to display the difference from one sample to the next. The counter value is a constantly increasing number  and the value displayed is the difference between the current value and the previous value. Negative numbers are not allowed which shouldn't be a problem as long as the counter value is increasing or unchanged.";
		break;
	case PERF_COUNTER_LARGE_DELTA:
		Name = "PERF_COUNTER_LARGE_DELTA";
		Description = "This counter is used to display the difference from one sample to the next. The counter value is a constantly increasing number  and the value displayed is the difference between the current value and the previous value. Negative numbers are not allowed which shouldn't be a problem as long as the counter value is increasing or unchanged.";
		break;
	case PERF_PRECISION_SYSTEM_TIMER:
		Name = "PERF_PRECISION_SYSTEM_TIMER";
		Description = "The precision counters are timers that consist of two counter values:\r\n\t1) the count of elapsed time of the event being monitored\r\n\t2) the \"clock\" time in the same units\r\nthe precision timers are used where the standard system timers are not precise enough for accurate readings. It's assumed that the service providing the data is also providing a timestamp at the same time which will eliminate any error that may occur since some small and variable time elapses between the time the system timestamp is captured and when the data is collected from the performance DLL. Only in extreme cases has this been observed to be problematic.\r\nwhen using this type of timer, the definition of the PERF_PRECISION_TIMESTAMP counter must immediately follow the definition of the PERF_PRECISION_*_TIMER in the Object header\r\nThe timer used has the same frequency as the System Performance Timer";
		break;
	case PERF_PRECISION_100NS_TIMER:
		Name = "PERF_PRECISION_100NS_TIMER";
		Description = "The precision counters are timers that consist of two counter values:\r\n\t1) the count of elapsed time of the event being monitored\r\n\t2) the \"clock\" time in the same units\r\nthe precision timers are used where the standard system timers are not precise enough for accurate readings. It's assumed that the service providing the data is also providing a timestamp at the same time which will eliminate any error that may occur since some small and variable time elapses between the time the system timestamp is captured and when the data is collected from the performance DLL. Only in extreme cases has this been observed to be problematic.\r\nwhen using this type of timer, the definition of the PERF_PRECISION_TIMESTAMP counter must immediately follow the definition of the PERF_PRECISION_*_TIMER in the Object header\r\nThe timer used has the same frequency as the 100 NanoSecond Timer";
		break;
	case PERF_PRECISION_OBJECT_TIMER:
		Name = "PERF_PRECISION_OBJECT_TIMER";
		Description = "The precision counters are timers that consist of two counter values:\r\n\t1) the count of elapsed time of the event being monitored\r\n\t2) the \"clock\" time in the same units\r\nthe precision timers are used where the standard system timers are not precise enough for accurate readings. It's assumed that the service providing the data is also providing a timestamp at the same time which will eliminate any error that may occur since some small and variable time elapses between the time the system timestamp is captured and when the data is collected from the performance DLL. Only in extreme cases has this been observed to be problematic.\r\nwhen using this type of timer, the definition of the PERF_PRECISION_TIMESTAMP counter must immediately follow the definition of the PERF_PRECISION_*_TIMER in the Object header\r\nThe timer used is of the frequency specified in the Object header's. PerfFreq field (PerfTime is ignored)";
		break;
	}
	strcpy(name, Name.c_str());
	strcpy(description, Description.c_str());
}