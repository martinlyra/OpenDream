﻿using OpenDreamServer.Dream.Objects;
using System;
using System.Collections.Generic;

namespace OpenDreamServer.Dream.Procs {
    class DreamProcScope {
        public DreamProcScope ParentScope;
        public DreamProc SuperProc;
        public DreamObject DreamObject;
        public DreamObject Usr;

        private Dictionary<string, DreamValue> Variables = null;

        public DreamProcScope(DreamObject dreamObject, DreamObject usr) {
            ParentScope = null;
            SuperProc = null;
            DreamObject = dreamObject;
            Usr = usr;
        }

        public DreamProcScope(DreamProcScope parentScope) {
            ParentScope = parentScope;
            SuperProc = parentScope.SuperProc;
            DreamObject = parentScope.DreamObject;
            Usr = parentScope.Usr;
        }

        public DreamValue GetValue(string valueName) {
            if (Variables != null && Variables.TryGetValue(valueName, out DreamValue value)) {
                return value;
            } else if (ParentScope != null) {
                return ParentScope.GetValue(valueName);
            } else if (DreamObject != null && DreamObject.TryGetVariable(valueName, out value)) {
                return value;
            } else if (DreamObject != null && DreamObject.ObjectDefinition.HasGlobalVariable(valueName)) {
                return DreamObject.ObjectDefinition.GetGlobalVariable(valueName).Value;
            } else {
                throw new Exception("Value '" + valueName + "' doesn't exist");
            }
        }

        public DreamValue GetProc(string procName) {
            if (DreamObject != null && DreamObject.TryGetProc(procName, out DreamProc proc)) {
                return new DreamValue(proc);
            } else {
                throw new Exception("Proc '" + procName + "' doesn't exist");
            }
        }

        public void AssignValue(string valueName, DreamValue value) {
            if (Variables != null && Variables.ContainsKey(valueName)) {
                Variables[valueName] = value;
            } else if (ParentScope != null) {
                ParentScope.AssignValue(valueName, value);
            } else if (DreamObject != null && DreamObject.HasVariable(valueName)) {
                DreamObject.SetVariable(valueName, value);
            } else if (DreamObject != null && DreamObject.ObjectDefinition.HasGlobalVariable(valueName)) {
                DreamObject.ObjectDefinition.GetGlobalVariable(valueName).Value = value;
            } else {
                throw new Exception("Value '" + valueName + "' doesn't exist");
            }
        }

        public void CreateVariable(string name, DreamValue value) {
            if (Variables == null) Variables = new Dictionary<string, DreamValue>();

            Variables.Add(name, value);
        }
    }
}
