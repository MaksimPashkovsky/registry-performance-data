#pragma once
#include <winperf.h>
extern "C" __declspec(dllexport) void GetDataBlockPointer(PPERF_DATA_BLOCK* p);

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
	DWORD* Version);

extern "C" __declspec(dllexport) void GetFirstObjectTypePointer(PPERF_DATA_BLOCK p, PPERF_OBJECT_TYPE* pp);

extern "C" __declspec(dllexport) void GetNextObjectTypePointer(PPERF_OBJECT_TYPE p, PPERF_OBJECT_TYPE* pp);

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
	LONGLONG* PerfFreq);

extern "C" __declspec(dllexport) void GetCounterDefinitionPointer(PPERF_OBJECT_TYPE p, PPERF_COUNTER_DEFINITION * pp);

extern "C" __declspec(dllexport) void GetCounterBlock_InstPointer(PPERF_INSTANCE_DEFINITION p, PPERF_COUNTER_BLOCK * pp);

extern "C" __declspec(dllexport) void GetCounterBlock_ObjPointer(PPERF_OBJECT_TYPE p, PPERF_COUNTER_BLOCK * pp);

extern "C" __declspec(dllexport) void GetFirstInstancePointer(PPERF_OBJECT_TYPE p, PPERF_INSTANCE_DEFINITION* pp);

extern "C" __declspec(dllexport) void GetNextInstancePointer(PPERF_COUNTER_BLOCK p, PPERF_INSTANCE_DEFINITION* pp);

extern "C" __declspec(dllexport) void GetInstanceInfo(
	PPERF_INSTANCE_DEFINITION p,
	DWORD CodePage,
	DWORD* ByteLength,
	DWORD* ParentObjectTitleIndex,
	DWORD* ParentObjectInstance,
	WCHAR* Name);

extern "C" __declspec(dllexport) void GetCounterInfo(
	PPERF_COUNTER_DEFINITION p,
	DWORD * ByteLength,
	DWORD * CounterHelpTitleIndex,
	DWORD * CounterNameTitleIndex,
	DWORD * CounterSize,
	DWORD * CounterType,
	LONG * DefaultScale,
	DWORD * DetailLevel);

extern "C" __declspec(dllexport) void GetCounterValue(
	PPERF_DATA_BLOCK g_pPerfDataHead,
	PPERF_OBJECT_TYPE pObject,
	PPERF_COUNTER_DEFINITION pCounter,
	PPERF_COUNTER_BLOCK pCounterDataBlock,
	ULONGLONG* Data,
	LONGLONG* Time,
	DWORD* MultiCounterData,
	LONGLONG* Frequency);

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
	wchar_t* FinalValue);

extern "C" __declspec(dllexport) void GetNextCounterPointer(PPERF_COUNTER_DEFINITION p, PPERF_COUNTER_DEFINITION * pp);

extern "C" __declspec(dllexport) void GetCounterType(int numbertype, char* name, char* description);