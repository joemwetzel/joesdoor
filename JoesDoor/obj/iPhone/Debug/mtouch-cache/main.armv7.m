#include "monotouch/main.h"

extern void *mono_aot_module_JoesDoor_info;
extern void *mono_aot_module_monotouch_info;
extern void *mono_aot_module_mscorlib_info;
extern void *mono_aot_module_System_info;
extern void *mono_aot_module_System_Core_info;
extern void *mono_aot_module_System_Xml_info;
extern void *mono_aot_module_RestSharp_MonoTouch_info;
extern void *mono_aot_module_System_Xml_Linq_info;

void monotouch_register_modules ()
{
	mono_aot_register_module (mono_aot_module_JoesDoor_info);
	mono_aot_register_module (mono_aot_module_monotouch_info);
	mono_aot_register_module (mono_aot_module_mscorlib_info);
	mono_aot_register_module (mono_aot_module_System_info);
	mono_aot_register_module (mono_aot_module_System_Core_info);
	mono_aot_register_module (mono_aot_module_System_Xml_info);
	mono_aot_register_module (mono_aot_module_RestSharp_MonoTouch_info);
	mono_aot_register_module (mono_aot_module_System_Xml_Linq_info);

}

void monotouch_register_assemblies ()
{
	monotouch_open_and_register ("monotouch.dll");

}

void monotouch_setup ()
{
	use_old_dynamic_registrar = TRUE;
	monotouch_create_classes ();
	monotouch_assembly_name = "JoesDoor.exe";
	mono_use_llvm = FALSE;
	monotouch_log_level = 0;
	monotouch_debug_mode = TRUE;
	monotouch_new_refcount = FALSE;
	monotouch_sgen = TRUE;
	setenv ("MONO_GC_PARAMS", "nursery-size=512k", 1);
}

