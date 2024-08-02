#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>


template <typename T1>
struct InterfaceActionInvoker1
{
	typedef void (*Action)(void*, T1, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		((Action)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
template <typename R>
struct InterfaceFuncInvoker0
{
	typedef R (*Func)(void*, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		return ((Func)invokeData.methodPtr)(obj, invokeData.method);
	}
};

struct IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832;
struct StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF;
struct IDictionary_t6D03155AF1FA9083817AA5B6AD7DEEACC26AB220;
struct IUserIdentifierProvider_t37955FEADD673B2F4333F6D756F9F9BEC3486E41;
struct InstallationId_tA5712B394172DB4DBA3B5F9A67D5522D37E2B873;
struct SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6;
struct String_t;
struct UnityAdsIdentifier_t8D3A8D6DB9A1FB469E2E7F3D58B30C3D0D71DA56;
struct UnityAnalyticsIdentifier_tAB8DD85C666D878FC108C324E10DF6528B5BDA5D;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;

IL2CPP_EXTERN_C RuntimeClass* Exception_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IUserIdentifierProvider_t37955FEADD673B2F4333F6D756F9F9BEC3486E41_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* UnityAdsIdentifier_t8D3A8D6DB9A1FB469E2E7F3D58B30C3D0D71DA56_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* UnityAnalyticsIdentifier_tAB8DD85C666D878FC108C324E10DF6528B5BDA5D_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral20353326ACF03BB4112F3080DBCA0AC47DBC7BB2;
IL2CPP_EXTERN_C String_t* _stringLiteral9802A0C242B6B1F11EC74461BB2DF09CE62B8035;
struct Exception_t_marshaled_com;
struct Exception_t_marshaled_pinvoke;


IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct U3CModuleU3E_tC6F3BE26E8DBB3B8108F8360B026950D37DAF754 
{
};
struct InstallationId_tA5712B394172DB4DBA3B5F9A67D5522D37E2B873  : public RuntimeObject
{
	String_t* ___Identifier;
	RuntimeObject* ___UnityAdsIdentifierProvider;
	RuntimeObject* ___UnityAnalyticsIdentifierProvider;
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
};
struct UnityAdsIdentifier_t8D3A8D6DB9A1FB469E2E7F3D58B30C3D0D71DA56  : public RuntimeObject
{
};
struct UnityAnalyticsIdentifier_tAB8DD85C666D878FC108C324E10DF6528B5BDA5D  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Guid_t 
{
	int32_t ____a;
	int16_t ____b;
	int16_t ____c;
	uint8_t ____d;
	uint8_t ____e;
	uint8_t ____f;
	uint8_t ____g;
	uint8_t ____h;
	uint8_t ____i;
	uint8_t ____j;
	uint8_t ____k;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
struct Exception_t  : public RuntimeObject
{
	String_t* ____className;
	String_t* ____message;
	RuntimeObject* ____data;
	Exception_t* ____innerException;
	String_t* ____helpURL;
	RuntimeObject* ____stackTrace;
	String_t* ____stackTraceString;
	String_t* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	RuntimeObject* ____dynamicMethods;
	int32_t ____HResult;
	String_t* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_pinvoke
{
	char* ____className;
	char* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_pinvoke* ____innerException;
	char* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	char* ____stackTraceString;
	char* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	char* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_com
{
	Il2CppChar* ____className;
	Il2CppChar* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_com* ____innerException;
	Il2CppChar* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	Il2CppChar* ____stackTraceString;
	Il2CppChar* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	Il2CppChar* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct Guid_t_StaticFields
{
	Guid_t ___Empty;
};
struct Exception_t_StaticFields
{
	RuntimeObject* ___s_EDILock;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif



IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2 (RuntimeObject* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityAdsIdentifier__ctor_mFD5085EB0AD902D689B820D13D697CE640E2E7B8 (UnityAdsIdentifier_t8D3A8D6DB9A1FB469E2E7F3D58B30C3D0D71DA56* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityAnalyticsIdentifier__ctor_mE19B73CB1414B5AC0619E8EF1366C0164B06D14B (UnityAnalyticsIdentifier_tAB8DD85C666D878FC108C324E10DF6528B5BDA5D* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478 (String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void InstallationId_CreateIdentifier_mC089486FCD96983C9186689C41D1D75E36EFB90C (InstallationId_tA5712B394172DB4DBA3B5F9A67D5522D37E2B873* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* InstallationId_ReadIdentifierFromFile_m126AF7B4F947DBB382AA8848D51FEA52C91C3E7C (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* InstallationId_GenerateGuid_m7220D8C5AE5B4DCA4F4C01A6F921FF66C23E855A (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void InstallationId_WriteIdentifierToFile_m67273AD8773FDBEE468B0BB7F502B3383AB9A6C4 (String_t* ___0_identifier, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* PlayerPrefs_GetString_mA4C9F842BF77E5572AB20EA087C7048F870D02AE (String_t* ___0_key, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void PlayerPrefs_SetString_mF4F457C81BB75F0213547C6287BA36E15E1F0256 (String_t* ___0_key, String_t* ___1_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void PlayerPrefs_Save_m82567E045D69C838112EA204B60C144D4C1EA3AE (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Guid_t Guid_NewGuid_m1F4894E8DC089811D6252148AD5858E58D43A7BD (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Guid_ToString_m2BFFD5FA726E03FA707AAFCCF065896C46D5290C (Guid_t* __this, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// Method Definition Index: 47929
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void InstallationId__ctor_mD8E8D5B7A72C54F2517D33F9F2C89FA708CC6BF7 (InstallationId_tA5712B394172DB4DBA3B5F9A67D5522D37E2B873* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityAdsIdentifier_t8D3A8D6DB9A1FB469E2E7F3D58B30C3D0D71DA56_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&UnityAnalyticsIdentifier_tAB8DD85C666D878FC108C324E10DF6528B5BDA5D_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:16>
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:18>
		UnityAdsIdentifier_t8D3A8D6DB9A1FB469E2E7F3D58B30C3D0D71DA56* L_0 = (UnityAdsIdentifier_t8D3A8D6DB9A1FB469E2E7F3D58B30C3D0D71DA56*)il2cpp_codegen_object_new(UnityAdsIdentifier_t8D3A8D6DB9A1FB469E2E7F3D58B30C3D0D71DA56_il2cpp_TypeInfo_var);
		UnityAdsIdentifier__ctor_mFD5085EB0AD902D689B820D13D697CE640E2E7B8(L_0, NULL);
		__this->___UnityAdsIdentifierProvider = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___UnityAdsIdentifierProvider), (void*)L_0);
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:19>
		UnityAnalyticsIdentifier_tAB8DD85C666D878FC108C324E10DF6528B5BDA5D* L_1 = (UnityAnalyticsIdentifier_tAB8DD85C666D878FC108C324E10DF6528B5BDA5D*)il2cpp_codegen_object_new(UnityAnalyticsIdentifier_tAB8DD85C666D878FC108C324E10DF6528B5BDA5D_il2cpp_TypeInfo_var);
		UnityAnalyticsIdentifier__ctor_mE19B73CB1414B5AC0619E8EF1366C0164B06D14B(L_1, NULL);
		__this->___UnityAnalyticsIdentifierProvider = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___UnityAnalyticsIdentifierProvider), (void*)L_1);
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:20>
		return;
	}
}
// Method Definition Index: 47930
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* InstallationId_GetOrCreateIdentifier_mB808E735A25CB3357E33337F9A4B737DDAB6B40F (InstallationId_tA5712B394172DB4DBA3B5F9A67D5522D37E2B873* __this, const RuntimeMethod* method) 
{
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:24>
		String_t* L_0 = __this->___Identifier;
		bool L_1;
		L_1 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_0, NULL);
		if (!L_1)
		{
			goto IL_0013;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:25>
		InstallationId_CreateIdentifier_mC089486FCD96983C9186689C41D1D75E36EFB90C(__this, NULL);
	}

IL_0013:
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:27>
		String_t* L_2 = __this->___Identifier;
		return L_2;
	}
}
// Method Definition Index: 47931
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void InstallationId_CreateIdentifier_mC089486FCD96983C9186689C41D1D75E36EFB90C (InstallationId_tA5712B394172DB4DBA3B5F9A67D5522D37E2B873* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IUserIdentifierProvider_t37955FEADD673B2F4333F6D756F9F9BEC3486E41_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	String_t* V_0 = NULL;
	String_t* V_1 = NULL;
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:32>
		String_t* L_0;
		L_0 = InstallationId_ReadIdentifierFromFile_m126AF7B4F947DBB382AA8848D51FEA52C91C3E7C(NULL);
		__this->___Identifier = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Identifier), (void*)L_0);
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:33>
		String_t* L_1 = __this->___Identifier;
		bool L_2;
		L_2 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_1, NULL);
		if (L_2)
		{
			goto IL_0019;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:34>
		return;
	}

IL_0019:
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:36>
		RuntimeObject* L_3 = __this->___UnityAnalyticsIdentifierProvider;
		String_t* L_4;
		L_4 = InterfaceFuncInvoker0< String_t* >::Invoke(0, IUserIdentifierProvider_t37955FEADD673B2F4333F6D756F9F9BEC3486E41_il2cpp_TypeInfo_var, L_3);
		V_0 = L_4;
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:37>
		RuntimeObject* L_5 = __this->___UnityAdsIdentifierProvider;
		String_t* L_6;
		L_6 = InterfaceFuncInvoker0< String_t* >::Invoke(0, IUserIdentifierProvider_t37955FEADD673B2F4333F6D756F9F9BEC3486E41_il2cpp_TypeInfo_var, L_5);
		V_1 = L_6;
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:39>
		String_t* L_7 = V_0;
		bool L_8;
		L_8 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_7, NULL);
		if (L_8)
		{
			goto IL_0042;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:41>
		String_t* L_9 = V_0;
		__this->___Identifier = L_9;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Identifier), (void*)L_9);
		goto IL_005e;
	}

IL_0042:
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:43>
		String_t* L_10 = V_1;
		bool L_11;
		L_11 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_10, NULL);
		if (L_11)
		{
			goto IL_0053;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:45>
		String_t* L_12 = V_1;
		__this->___Identifier = L_12;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Identifier), (void*)L_12);
		goto IL_005e;
	}

IL_0053:
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:49>
		String_t* L_13;
		L_13 = InstallationId_GenerateGuid_m7220D8C5AE5B4DCA4F4C01A6F921FF66C23E855A(NULL);
		__this->___Identifier = L_13;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Identifier), (void*)L_13);
	}

IL_005e:
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:52>
		String_t* L_14 = __this->___Identifier;
		InstallationId_WriteIdentifierToFile_m67273AD8773FDBEE468B0BB7F502B3383AB9A6C4(L_14, NULL);
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:54>
		String_t* L_15 = V_0;
		bool L_16;
		L_16 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_15, NULL);
		if (!L_16)
		{
			goto IL_0082;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:56>
		RuntimeObject* L_17 = __this->___UnityAnalyticsIdentifierProvider;
		String_t* L_18 = __this->___Identifier;
		InterfaceActionInvoker1< String_t* >::Invoke(1, IUserIdentifierProvider_t37955FEADD673B2F4333F6D756F9F9BEC3486E41_il2cpp_TypeInfo_var, L_17, L_18);
	}

IL_0082:
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:59>
		String_t* L_19 = V_1;
		bool L_20;
		L_20 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_19, NULL);
		if (!L_20)
		{
			goto IL_009b;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:61>
		RuntimeObject* L_21 = __this->___UnityAdsIdentifierProvider;
		String_t* L_22 = __this->___Identifier;
		InterfaceActionInvoker1< String_t* >::Invoke(1, IUserIdentifierProvider_t37955FEADD673B2F4333F6D756F9F9BEC3486E41_il2cpp_TypeInfo_var, L_21, L_22);
	}

IL_009b:
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:63>
		return;
	}
}
// Method Definition Index: 47932
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* InstallationId_ReadIdentifierFromFile_m126AF7B4F947DBB382AA8848D51FEA52C91C3E7C (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral9802A0C242B6B1F11EC74461BB2DF09CE62B8035);
		s_Il2CppMethodInitialized = true;
	}
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:67>
		String_t* L_0;
		L_0 = PlayerPrefs_GetString_mA4C9F842BF77E5572AB20EA087C7048F870D02AE(_stringLiteral9802A0C242B6B1F11EC74461BB2DF09CE62B8035, NULL);
		return L_0;
	}
}
// Method Definition Index: 47933
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void InstallationId_WriteIdentifierToFile_m67273AD8773FDBEE468B0BB7F502B3383AB9A6C4 (String_t* ___0_identifier, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral9802A0C242B6B1F11EC74461BB2DF09CE62B8035);
		s_Il2CppMethodInitialized = true;
	}
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:72>
		String_t* L_0 = ___0_identifier;
		PlayerPrefs_SetString_mF4F457C81BB75F0213547C6287BA36E15E1F0256(_stringLiteral9802A0C242B6B1F11EC74461BB2DF09CE62B8035, L_0, NULL);
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:73>
		PlayerPrefs_Save_m82567E045D69C838112EA204B60C144D4C1EA3AE(NULL);
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:74>
		return;
	}
}
// Method Definition Index: 47934
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* InstallationId_GenerateGuid_m7220D8C5AE5B4DCA4F4C01A6F921FF66C23E855A (const RuntimeMethod* method) 
{
	Guid_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/InstallationId.cs:78>
		Guid_t L_0;
		L_0 = Guid_NewGuid_m1F4894E8DC089811D6252148AD5858E58D43A7BD(NULL);
		V_0 = L_0;
		String_t* L_1;
		L_1 = Guid_ToString_m2BFFD5FA726E03FA707AAFCCF065896C46D5290C((&V_0), NULL);
		return L_1;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// Method Definition Index: 47937
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* UnityAdsIdentifier_get_UserId_m4E41B6AEB6405B6BFA7D4E52FE3F66478A18D90A (UnityAdsIdentifier_t8D3A8D6DB9A1FB469E2E7F3D58B30C3D0D71DA56* __this, const RuntimeMethod* method) 
{
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/UserIdProviders/UnityAdsIdentifier.cs:21>
		return (String_t*)NULL;
	}
}
// Method Definition Index: 47938
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityAdsIdentifier_set_UserId_mA2519CC19694D24C35D6E2E72B79E2CE34DACA4F (UnityAdsIdentifier_t8D3A8D6DB9A1FB469E2E7F3D58B30C3D0D71DA56* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/UserIdProviders/UnityAdsIdentifier.cs:31>
		return;
	}
}
// Method Definition Index: 47939
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityAdsIdentifier__ctor_mFD5085EB0AD902D689B820D13D697CE640E2E7B8 (UnityAdsIdentifier_t8D3A8D6DB9A1FB469E2E7F3D58B30C3D0D71DA56* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// Method Definition Index: 47940
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* UnityAnalyticsIdentifier_get_UserId_m15964ABDB88E2D752C471143ED9C5A32017989CE (UnityAnalyticsIdentifier_tAB8DD85C666D878FC108C324E10DF6528B5BDA5D* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral20353326ACF03BB4112F3080DBCA0AC47DBC7BB2);
		s_Il2CppMethodInitialized = true;
	}
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/UserIdProviders/UnityAnalyticsIdentifier.cs:12>
		String_t* L_0;
		L_0 = PlayerPrefs_GetString_mA4C9F842BF77E5572AB20EA087C7048F870D02AE(_stringLiteral20353326ACF03BB4112F3080DBCA0AC47DBC7BB2, NULL);
		return L_0;
	}
}
// Method Definition Index: 47941
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityAnalyticsIdentifier_set_UserId_m2751FA3CB0A777DAC8AA56ECBE8945EED55CAC3C (UnityAnalyticsIdentifier_tAB8DD85C666D878FC108C324E10DF6528B5BDA5D* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral20353326ACF03BB4112F3080DBCA0AC47DBC7BB2);
		s_Il2CppMethodInitialized = true;
	}
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	try
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/UserIdProviders/UnityAnalyticsIdentifier.cs:17>
		String_t* L_0 = ___0_value;
		PlayerPrefs_SetString_mF4F457C81BB75F0213547C6287BA36E15E1F0256(_stringLiteral20353326ACF03BB4112F3080DBCA0AC47DBC7BB2, L_0, NULL);
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/UserIdProviders/UnityAnalyticsIdentifier.cs:18>
		PlayerPrefs_Save_m82567E045D69C838112EA204B60C144D4C1EA3AE(NULL);
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/UserIdProviders/UnityAnalyticsIdentifier.cs:19>
		goto IL_0015;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_0012;
		}
		throw e;
	}

CATCH_0012:
	{
		Exception_t* L_1 = ((Exception_t*)IL2CPP_GET_ACTIVE_EXCEPTION(Exception_t*));;
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/UserIdProviders/UnityAnalyticsIdentifier.cs:20>
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/UserIdProviders/UnityAnalyticsIdentifier.cs:23>
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		goto IL_0015;
	}

IL_0015:
	{
		//<source_info:./Library/PackageCache/com.unity.services.core@1.12.5/Runtime/Device/UserIdProviders/UnityAnalyticsIdentifier.cs:24>
		return;
	}
}
// Method Definition Index: 47942
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnityAnalyticsIdentifier__ctor_mE19B73CB1414B5AC0619E8EF1366C0164B06D14B (UnityAnalyticsIdentifier_tAB8DD85C666D878FC108C324E10DF6528B5BDA5D* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
