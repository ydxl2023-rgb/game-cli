using UnityEngine;
using UnityEditor;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Reflection;
using UnityEditor.Compilation;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom;

namespace Oathx.Widget.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public static class WidgetInject
    {
        [InitializeOnLoadMethod]
        public static void WidgetStartup()
        {
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
        }

        [MenuItem("Widget/Inject")]
        public static void Inject()
        {
            var widgetMode = WidgetSettings.Instance.widgetMode;
            if (widgetMode == WidgetMode.Inject)
            {
                List<string> assemblys = new List<string>();

                var assemblyNames = WidgetSettings.Instance.injectAssemblyNames;
                if (assemblyNames != null)
                    assemblys.AddRange(assemblyNames);

                var assemblyDefinitionAssets = WidgetSettings.Instance.assemblyDefinitionAssets;
                if (assemblyDefinitionAssets != null)
                {
                    foreach (var assemblyDefinitionAsset in assemblyDefinitionAssets)
                    {
                        if (assemblyDefinitionAsset != null)
                            assemblys.Add(assemblyDefinitionAsset.name);
                    }
                }

                foreach (var name in assemblys)
                {
                    string injectAssembly = GetInjectAssemblyName(name);
                    string assemblyFile = injectAssembly.Replace("\\", "/");
                    if (assemblyFile == injectAssembly)
                    {
                        Inject(injectAssembly);
                    }
                }
            }
            else
            {
                EditorUtility.DisplayDialog("warning",
                    "Please change the widgetMode to Inject mode. The current mode used is source code generation mode, and changing the mode does not support inject", "ok");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetInjectAssemblyName(string name)
        {
            return $"Library/ScriptAssemblies/{name}.dll";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="compilerMessages"></param>
        public static void OnAssemblyCompilationFinished(string file, CompilerMessage[] compilerMessages)
        {
            var widgetMode = WidgetSettings.Instance.widgetMode;
            if (widgetMode == WidgetMode.Inject)
            {
                var assemblyNames = WidgetSettings.Instance.injectAssemblyNames;
                foreach (var assemblyName in assemblyNames)
                {
                    if (file.Contains(assemblyName))
                    {
                        Inject();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstUpper(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FriendlyName(this System.Type type)
        {
            using (var p = new CSharpCodeProvider())
            {
                var r = new CodeTypeReference(type);
                return p.GetTypeOutput(r);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static MethodReference GenericMethod(this MethodReference self, params TypeReference[] arguments)
        {
            if (self.GenericParameters.Count != arguments.Length)
                throw new System.ArgumentException();

            var instance = new GenericInstanceMethod(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool HasAttribute<T>(this PropertyDefinition property) where T : System.Attribute
        {
            var attributes = property.CustomAttributes;
            foreach (var attribute in attributes)
            {
                if (attribute.AttributeType.Name.Equals(typeof(T).Name))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="propertyIndex"></param>
        /// <returns></returns>
        public static Instruction CreateInstruction(ILProcessor worker, int propertyIndex)
        {
            switch (propertyIndex)
            {
                case 0: return worker.Create(OpCodes.Ldc_I4_0);
                case 1: return worker.Create(OpCodes.Ldc_I4_1);
                case 2: return worker.Create(OpCodes.Ldc_I4_2);
                case 3: return worker.Create(OpCodes.Ldc_I4_3);
                case 4: return worker.Create(OpCodes.Ldc_I4_4);
                case 5: return worker.Create(OpCodes.Ldc_I4_5);
                case 6: return worker.Create(OpCodes.Ldc_I4_6);
                case 7: return worker.Create(OpCodes.Ldc_I4_7);
                case 8: return worker.Create(OpCodes.Ldc_I4_8);

                default:
                    if (propertyIndex >= -128 && propertyIndex <= 127)
                    {
                        return worker.Create(OpCodes.Ldc_I4_S, (sbyte)propertyIndex);
                    }
                    else
                    {
                        return worker.Create(OpCodes.Ldc_I4, propertyIndex);
                    }
            }
        }

        /// <summary>
        /// inject function type
        /// </summary>
        public enum MethodType
        {
            Init,
            Set,
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyName"></param>
        public static void Inject(string assemblyName)
        {
            string assemblyPath = Path.Combine(Application.dataPath.Replace("Assets", ""), assemblyName); ;
            try
            {
                EditorApplication.LockReloadAssemblies();

                using (var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters()
                {
                    ReadSymbols = true,
                    SymbolReaderProvider = new Mono.Cecil.Pdb.PdbReaderProvider()
                }))
                {
                    if (InjectFunction(assembly.MainModule))
                    {
                        assembly.Write(assemblyPath, new WriterParameters()
                        {
                            WriteSymbols = true,
                            SymbolWriterProvider = new Mono.Cecil.Pdb.PdbWriterProvider()
                        });
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }

            Debug.Log($"Inject <color=yellow>{assemblyName}</color> done");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static bool InjectFunction(ModuleDefinition module)
        {
            var methodSet = module.ImportReference(typeof(VM).GetMethod(MethodType.Set.ToString(),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static));

            foreach (var type in module.Types)
            {
                if (type.BaseType != null && type.BaseType.Name.Equals(nameof(VM), System.StringComparison.Ordinal))
                {
                    Inject(module, type, methodSet);
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public static void Inject(ModuleDefinition module, TypeDefinition type, MethodReference method)
        {
            int propertyIndex = 0;

            foreach (var property in type.Properties)
            {
                bool inject = false;
                var attributes = property.CustomAttributes;
                foreach (var attribute in attributes)
                {
                    if (attribute.AttributeType.Name.Equals(typeof(BindingAttribute).Name))
                    {
                        inject = true;
                        break;
                    }
                }

                if (inject)
                {
                    if (InjectProcess(property, propertyIndex, method))
                        propertyIndex++;
                }
            }

            InjectInit(module, type, propertyIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        /// <param name="type"></param>
        /// <param name="propertyIndex"></param>
        public static void InjectInit(ModuleDefinition module, TypeDefinition type, int propertyIndex)
        {
            var methodInit = module.ImportReference(typeof(VM).GetMethod(MethodType.Init.ToString(),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static));

            var ctorMethods = type.Methods.Where(x => x.Name == ".ctor");
            foreach (var ctor in ctorMethods)
            {
                var worker = ctor.Body.GetILProcessor();
                if (worker != null)
                {
                    var ldcIdx = CreateInstruction(worker, propertyIndex);

                    // If the code has already been injected, first OpCode is Nop and  final OpCode is Nop
                    var ldcCount = ctor.Body.Instructions.Count;
                    if (ctor.Body.Instructions[0].OpCode == OpCodes.Nop && ctor.Body.Instructions[ldcCount - 2].OpCode == OpCodes.Nop)
                    {
                        ctor.Body.Instructions[ldcCount - 4] = ldcIdx;
                    }
                    else
                    {
                        var method = worker.Create(OpCodes.Call, methodInit);

                        var ldarg0 = worker.Create(OpCodes.Ldarg_0);
                        worker.InsertBefore(ctor.Body.Instructions[0],
                            worker.Create(OpCodes.Nop));
                        worker.InsertBefore(ctor.Body.Instructions[ctor.Body.Instructions.Count - 1], ldarg0);

                        // inject base Init method call
                        worker.InsertAfter(ldarg0, ldcIdx);
                        worker.InsertAfter(ldcIdx, method);
                        worker.InsertAfter(method, worker.Create(OpCodes.Nop));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyIndex"></param>
        /// <param name="baseMethod"></param>
        /// <returns></returns>
        public static bool InjectProcess(PropertyDefinition property, int propertyIndex, MethodReference baseMethod)
        {
            var body = property.SetMethod.Body;
            var worker = body.GetILProcessor();
            if (worker != null)
            {
                var ldcIdx = CreateInstruction(worker, propertyIndex);

                // If the code has already been injected, first OpCode is Nop and  final OpCode is Nop
                var ldcCount = body.Instructions.Count;
                if (body.Instructions[0].OpCode == OpCodes.Nop && body.Instructions[ldcCount - 2].OpCode == OpCodes.Nop)
                {
                    body.Instructions[ldcCount - 6] = ldcIdx;
                }
                else
                {
                    var method = worker.Create(OpCodes.Call, baseMethod.GenericMethod(property.PropertyType));
                    var ldarg0 = worker.Create(OpCodes.Ldarg_0);
                    var ldarg1 = worker.Create(OpCodes.Ldarg_1);

                    var ldname = worker.Create(OpCodes.Ldstr, property.Name);
                    var ldcPop = worker.Create(OpCodes.Pop);

                    worker.InsertBefore(body.Instructions[0],
                        worker.Create(OpCodes.Nop));
                    worker.InsertBefore(body.Instructions[body.Instructions.Count - 1], ldarg0);

                    // inject base Set method call
                    worker.InsertAfter(ldarg0, ldarg1);
                    worker.InsertAfter(ldarg1, ldcIdx);
                    worker.InsertAfter(ldcIdx, ldname);
                    worker.InsertAfter(ldname, method);
                    worker.InsertAfter(method, ldcPop);
                    worker.InsertAfter(ldcPop, worker.Create(OpCodes.Nop));
                }
            }

            return true;
        }
    }
}
