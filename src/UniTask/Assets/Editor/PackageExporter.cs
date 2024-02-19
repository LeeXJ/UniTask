#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class PackageExporter
{
    [MenuItem("Tools/Export Unitypackage")]
    public static void Export()
    {
        // 导出根目录
        var root = "Plugins/UniTask";
        // 获取根目录中的版本信息
        var version = GetVersion(root);

        // 根据版本信息构建导出文件名
        var fileName = string.IsNullOrEmpty(version) ? "UniTask.unitypackage" : $"UniTask.{version}.unitypackage";
        // 构建导出文件的路径
        var exportPath = "./" + fileName;

        // 获取根目录的绝对路径
        var path = Path.Combine(Application.dataPath, root);
        // 获取根目录下所有文件（包括子目录）的路径
        var assets = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            // 筛选出.cs、.asmdef、.json和.meta文件
            .Where(x => Path.GetExtension(x) == ".cs" || Path.GetExtension(x) == ".asmdef" || Path.GetExtension(x) == ".json" || Path.GetExtension(x) == ".meta")
            // 将文件路径转换为相对于Assets文件夹的路径
            .Select(x => "Assets" + x.Replace(Application.dataPath, "").Replace(@"\", "/"))
            .ToArray();

        // 打印将要导出的文件列表到Unity控制台
        UnityEngine.Debug.Log("Export below files" + Environment.NewLine + string.Join(Environment.NewLine, assets));

        // 导出Unity包
        AssetDatabase.ExportPackage(
            assets, // 要导出的文件列表
            exportPath, // 导出路径
            ExportPackageOptions.Default); // 导出选项

        // 打印导出完成的信息到Unity控制台
        UnityEngine.Debug.Log("Export complete: " + Path.GetFullPath(exportPath));
    }

    static string GetVersion(string root)
    {
        // 从环境变量中获取Unity包版本
        var version = Environment.GetEnvironmentVariable("UNITY_PACKAGE_VERSION");
        // 组合package.json文件的路径
        var versionJson = Path.Combine(Application.dataPath, root, "package.json");

        // 检查package.json文件是否存在
        if (File.Exists(versionJson))
        {
            // 读取package.json文件中的内容，并将其解析为Version对象
            var v = JsonUtility.FromJson<Version>(File.ReadAllText(versionJson));

            // 如果环境变量中的版本不为空
            if (!string.IsNullOrEmpty(version))
            {
                // 如果package.json中的版本与环境变量中的版本不匹配
                if (v.version != version)
                {
                    // 构建错误消息，指出版本不匹配
                    var msg = $"package.json and env version are mismatched. UNITY_PACKAGE_VERSION:{version}, package.json:{v.version}";

                    // 如果应用程序正在批处理模式下运行
                    if (Application.isBatchMode)
                    {
                        // 将错误消息打印到控制台并退出应用程序
                        Console.WriteLine(msg);
                        Application.Quit(1);
                    }

                    // 抛出异常，指示package.json和环境变量中的版本不匹配
                    throw new Exception("package.json and env version are mismatched.");
                }
            }

            // 更新版本为从package.json文件中获取的版本
            version = v.version;
        }

        // 返回获取到的版本信息
        return version;
    }

    public class Version
    {
        public string version;
    }
}

#endif