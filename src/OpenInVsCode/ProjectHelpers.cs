using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenInVsCode
{
    internal static class ProjectHelpers
    {
        public static string[] GetSelectedPaths(DTE2 dte, bool openSolutionProjectAsRegularFile)
        {
            var items = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;
            var paths = new List<string>();

            foreach (UIHierarchyItem selItem in items)
            {
                switch (selItem.Object)
                {
                    case ProjectItem projectItem:
                        paths.Add(projectItem.GetFilePath());
                        break;

                    case Project project:
                        paths.Add(openSolutionProjectAsRegularFile ? project.FileName : project.GetRootFolder());
                        break;

                    case Solution solution:
                        paths.Add(openSolutionProjectAsRegularFile
                            ? solution.FullName
                            : Path.GetDirectoryName(solution.FileName));
                        break;
                }
            }

            return paths.ToArray();
        }

        public static string GetFilePath(this ProjectItem item)
        {
            return item.FileNames[1];
        }

        public static string GetRootFolder(this Project project)
        {
            if (string.IsNullOrEmpty(project.FullName))
                return null;

            string fullPath;

            try
            {
                fullPath = project.Properties.Item("FullPath").Value as string;
            }
            catch (ArgumentException)
            {
                try
                {
                    // MFC projects don't have FullPath, and there seems to be no way to query existence
                    fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                }
                catch (ArgumentException)
                {
                    // Installer projects have a ProjectPath.
                    fullPath = project.Properties.Item("ProjectPath").Value as string;
                }
            }

            if (string.IsNullOrEmpty(fullPath))
                return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : null;

            if (Directory.Exists(fullPath))
                return fullPath;

            if (File.Exists(fullPath))
                return Path.GetDirectoryName(fullPath);

            return null;
        }
    }
}