#pragma once
#include <winperf.h>
extern "C" __declspec(dllexport) int GetDataBlock(PPERF_DATA_BLOCK* p);

extern "C" __declspec(dllexport) int GetDataBlockInfo(PPERF_DATA_BLOCK p,
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

extern "C" __declspec(dllexport) int GetFirstObjectType(PPERF_DATA_BLOCK p, PPERF_OBJECT_TYPE* pp);

extern "C" __declspec(dllexport) int GetNextObjectType(PPERF_OBJECT_TYPE p, PPERF_OBJECT_TYPE* pp);

extern "C" __declspec(dllexport) int GetPerfObjectTypeInfo(PPERF_OBJECT_TYPE p,
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

extern "C" __declspec(dllexport) int GetCounterDefinition(PPERF_OBJECT_TYPE p, PPERF_COUNTER_DEFINITION * pp);

extern "C" __declspec(dllexport) int GetCounterBlock_Inst(PPERF_INSTANCE_DEFINITION p, PPERF_COUNTER_BLOCK * pp);

extern "C" __declspec(dllexport) int GetCounterBlock_Obj(PPERF_OBJECT_TYPE p, PPERF_COUNTER_BLOCK * pp);

extern "C" __declspec(dllexport) int GetFirstInstance(PPERF_OBJECT_TYPE p, PPERF_INSTANCE_DEFINITION* pp);

extern "C" __declspec(dllexport) int GetNextInstance(PPERF_COUNTER_BLOCK p, PPERF_INSTANCE_DEFINITION* pp);

extern "C" __declspec(dllexport) int GetInstanceInfo(PPERF_INSTANCE_DEFINITION p,
	DWORD CodePage,
	DWORD* ByteLength,
	DWORD* ParentObjectTitleIndex,
	DWORD* ParentObjectInstance,
	WCHAR* Name);

extern "C" __declspec(dllexport) int GetCounterInfo(PPERF_COUNTER_DEFINITION p,
	DWORD * ByteLength,
	DWORD * CounterHelpTitleIndex,
	DWORD * CounterNameTitleIndex,
	DWORD * CounterSize,
	DWORD * CounterType,
	LONG * DefaultScale,
	DWORD * DetailLevel);

extern "C" __declspec(dllexport) BOOL GetCounterValue(PPERF_DATA_BLOCK g_pPerfDataHead,
	PPERF_OBJECT_TYPE pObject,
	PPERF_COUNTER_DEFINITION pCounter,
	PPERF_COUNTER_BLOCK pCounterDataBlock,
	ULONGLONG* Data,
	LONGLONG* Time,
	DWORD* MultiCounterData,
	LONGLONG* Frequency);

extern "C" __declspec(dllexport) BOOL GetCalculatedValue(DWORD CounterType0,
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

extern "C" __declspec(dllexport) int GetNextCounter(PPERF_COUNTER_DEFINITION p, PPERF_COUNTER_DEFINITION * pp);

extern "C" __declspec(dllexport) int GetCounterType(int numbertype, char* name, char* description);