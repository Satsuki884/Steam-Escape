using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LinkModule.Editor.NoiseGenerator
{
    public class NoiseCodeGenerator : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private const string NOISE_FOLDER = "Assets/LinkModule/Scripts/InternalGameLogic";

        private static readonly Dictionary<string, List<string>> InterfaceMethodMap = new();
        private static readonly Dictionary<string, List<string>> ClassMethodMap = new();

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("[NoiseCodeGenerator] Generating noise code before build...");
            GenerateNoiseCode();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (Directory.Exists(NOISE_FOLDER))
            {
                Directory.Delete(NOISE_FOLDER, true);
                File.Delete(NOISE_FOLDER + ".meta");
            }

            AssetDatabase.Refresh();
            Debug.Log("[NoiseCodeCleaner] Removed noise code after build.");
        }

        //[MenuItem("Tools/Generate Noise Code")]
        private static void GenerateNoiseCode()
        {
            if (Directory.Exists(NOISE_FOLDER))
                Directory.Delete(NOISE_FOLDER, true);

            Directory.CreateDirectory(NOISE_FOLDER);

            int interfaceCount = Random.Range(5, 10);
            int classCount = Random.Range(10, 50);

            var interfaces = new List<string>();
            var allClassNames = new List<string>();

            // Generate interfaces
            for (int i = 0; i < interfaceCount; i++)
            {
                string interfaceName = $"IInternalGameLogic_{RandomString(6)}";
                interfaces.Add(interfaceName);
                string interfaceCode = GenerateInterface(interfaceName);
                File.WriteAllText(Path.Combine(NOISE_FOLDER, $"{interfaceName}.cs"), interfaceCode);
            }

            // Generate classes
            for (int i = 0; i < classCount; i++)
            {
                string className = $"InternalGameLogic_{RandomString(6)}";
                allClassNames.Add(className);

                string interfaceToImplement = interfaces[Random.Range(0, interfaces.Count)];
                string classCode = GenerateClass(className, interfaceToImplement);
                File.WriteAllText(Path.Combine(NOISE_FOLDER, $"{className}.cs"), classCode);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[NoiseCodeGenerator] Generated {classCount} classes and {interfaceCount} interfaces.");
        }

        private static string GenerateInterface(string interfaceName)
        {
            int methodCount = Random.Range(2, 4);
            var methodNames = new List<string>();
            var sb = new StringBuilder();

            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"public interface {interfaceName}");
            sb.AppendLine("{");

            for (int i = 0; i < methodCount; i++)
            {
                string methodName = RandomMethodName();
                methodNames.Add(methodName);
                sb.AppendLine($"    void {methodName}();");
            }

            sb.AppendLine("}");
            InterfaceMethodMap[interfaceName] = methodNames;

            return sb.ToString();
        }

        private static string GenerateClass(string className, string interfaceName)
        {
            var sb = new StringBuilder();
            int fieldCount = Random.Range(2, 4);
            var methodNames = new List<string>();

            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"public class {className} : {interfaceName}");
            sb.AppendLine("{");

            // Fields
            for (int i = 0; i < fieldCount; i++)
            {
                string type = Random.value > 0.5f ? "int" : "string";
                string value = type == "int"
                    ? Random.Range(1, 100).ToString()
                    : $"\"{RandomString(5)}\"";
                sb.AppendLine($"    private {type} _{RandomString(5)} = {value};");
            }

            sb.AppendLine();

            // Methods from interface
            if (InterfaceMethodMap.TryGetValue(interfaceName, out var interfaceMethods))
            {
                foreach (var methodName in interfaceMethods)
                {
                    methodNames.Add(methodName);
                    sb.AppendLine($"    public void {methodName}()");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        Debug.Log(\"{className}.{methodName} executing...\");");

                    if (Random.value > 0.4f)
                    {
                        sb.AppendLine($"        int x = {Random.Range(100, 999)};");
                        sb.AppendLine("        if (x % 2 == 0) Debug.Log(\"Even\"); else Debug.Log(\"Odd\");");
                    }

                    // Call another noise class method
                    if (ClassMethodMap.Count > 1 && Random.value > 0.5f)
                    {
                        var other = GetRandomClassWithMethods(className);
                        if (other != null)
                        {
                            var (otherClass, otherMethods) = other.Value;
                            var otherMethod = otherMethods[Random.Range(0, otherMethods.Count)];
                            sb.AppendLine($"        new {otherClass}().{otherMethod}();");
                        }
                    }

                    sb.AppendLine("    }");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("}");
            ClassMethodMap[className] = methodNames;

            return sb.ToString();
        }

        private static (string className, List<string> methods)? GetRandomClassWithMethods(string exclude)
        {
            var eligible = new List<string>(ClassMethodMap.Keys);
            eligible.Remove(exclude);
            if (eligible.Count == 0) return null;

            string selected = eligible[Random.Range(0, eligible.Count)];
            return (selected, ClassMethodMap[selected]);
        }

        private static string RandomString(int length)
        {
            const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(CHARS[Random.Range(0, CHARS.Length)]);
            return sb.ToString();
        }

        private static string RandomMethodName()
        {
            string[] verbs = { "Do", "Make", "Run", "Calc", "Handle", "Trigger", "Log", "Spin" };
            return verbs[Random.Range(0, verbs.Length)] + RandomString(5);
        }
    }
}