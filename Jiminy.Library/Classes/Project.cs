﻿using static Jiminy.Classes.Enumerations;

namespace Jiminy.Classes
{
    internal class Project
    {
        public Project(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }
            else
            {
                throw new Exception($"Project constructor name cannot be empty");
            }
        }

        public string Name { get; private set; }
        public enPriority Priority { get; set; } = enPriority.Low;
    }

    internal class ProjectRegistry
    {
        public ProjectRegistry()
        {

        }

        public ProjectRegistry(List<string> projectNames)
        {
            foreach (var name in projectNames)
            {
                Projects.Add(new Project(name));
            }
        }

        public List<Project> Projects { get; set; } = new();
    }
}