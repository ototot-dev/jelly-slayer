﻿using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Planning
{
    public partial class FieldPlanner
    {
        private List<FieldVariable> internalValueVariables = null;

        public void PrepareInternalValueVariables()
        {
            if (internalValueVariables != null) internalValueVariables.Clear();
            savedValueVariables.Clear();
        }

        public void SetInternalValueVariable(string name, object targetValue)
        {
            if (internalValueVariables == null)
            {
                GenerateInternalValueVariable(name, targetValue);
                return;
            }

            for (int i = 0; i < internalValueVariables.Count; i++)
            {
                var intVars = internalValueVariables[i];

                if (intVars == null) continue;

                if (intVars.Name == name)
                {
                    intVars.SetValue(targetValue);
                    return;
                }
            }

            GenerateInternalValueVariable(name, targetValue);
        }

        public bool ContainsInternalValueOfName(string name)
        {
            if (internalValueVariables == null) return false;

            for (int i = 0; i < internalValueVariables.Count; i++)
            {
                var intVars = internalValueVariables[i];
                if (intVars == null) continue;
                if (intVars.Name == name) return true;
            }

            return false;
        }

        public FieldVariable GetInternalValueVariable(string name, object initialValue, bool generateIfull = true)
        {
            if (internalValueVariables == null)
            {
                if(!generateIfull) if (initialValue == null) return null;
                return GenerateInternalValueVariable(name, initialValue);
            }

            for (int i = 0; i < internalValueVariables.Count; i++)
            {
                var intVars = internalValueVariables[i];
                if (intVars == null) continue;
                if (intVars.Name == name) return intVars;
            }

            if (!generateIfull) if (initialValue == null) return null;
            return GenerateInternalValueVariable(name, initialValue);
        }

        private FieldVariable GenerateInternalValueVariable(string name, object initialValue)
        {
            FieldVariable nVar = new FieldVariable(name, initialValue);
            nVar = AddInternalValueVariable(nVar);
            return nVar;
        }

        public FieldVariable AddInternalValueVariable(FieldVariable variable)
        {
            if (variable == null) return null;
            if (internalValueVariables == null) internalValueVariables = new List<FieldVariable>();

            for (int i = 0; i < internalValueVariables.Count; i++)
            {
                var cvar = internalValueVariables[i];
                if (variable == cvar) return cvar;
                if (variable.Name == cvar.Name) return cvar;
            }

            internalValueVariables.Add(variable);
            return variable;
        }


        // Saved internal values

        /// <summary> Values which can be saved during build planner generating stage, and being read after build planner executor generating </summary>
        public List<FieldVariable> SavedValueVariables { get { return savedValueVariables; } }
        [SerializeField, HideInInspector] private List<FieldVariable> savedValueVariables = new List<FieldVariable>();


        public void SetSavedInternalValueVariable(string name, object targetValue)
        {
            for (int i = 0; i < savedValueVariables.Count; i++)
            {
                var intVars = savedValueVariables[i];

                if (intVars == null) continue;

                if (intVars.Name == name)
                {
                    intVars.SetValue(targetValue);
                    return;
                }
            }

            GenerateSavedInternalValueVariable(name, targetValue);
        }

        public bool ContainsSavedInternalValueOfName(string name)
        {
            if (savedValueVariables == null) return false;

            for (int i = 0; i < savedValueVariables.Count; i++)
            {
                var intVars = savedValueVariables[i];
                if (intVars == null) continue;
                if (intVars.Name == name) return true;
            }

            return false;
        }

        public FieldVariable GetSavedInternalValueVariable(string name, object initialValue, bool generateIfull = true)
        {
            if (savedValueVariables == null)
            {
                if (!generateIfull) if (initialValue == null) return null;
                return GenerateSavedInternalValueVariable(name, initialValue);
            }

            for (int i = 0; i < savedValueVariables.Count; i++)
            {
                var intVars = savedValueVariables[i];
                if (intVars == null) continue;
                if (intVars.Name == name) return intVars;
            }

            if (!generateIfull) if (initialValue == null) return null;
            return GenerateSavedInternalValueVariable(name, initialValue);
        }

        private FieldVariable GenerateSavedInternalValueVariable(string name, object initialValue)
        {
            FieldVariable nVar = new FieldVariable(name, initialValue);
            nVar = AddSavedInternalValueVariable(nVar);
            return nVar;
        }

        public FieldVariable AddSavedInternalValueVariable(FieldVariable variable)
        {
            if (variable == null) return null;
            if (savedValueVariables == null) savedValueVariables = new List<FieldVariable>();

            for (int i = 0; i < savedValueVariables.Count; i++)
            {
                var cvar = savedValueVariables[i];
                if (variable == cvar) return cvar;
                if (variable.Name == cvar.Name) return cvar;
            }

            savedValueVariables.Add(variable);
            return variable;
        }


    }
}