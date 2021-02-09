﻿using OpenDreamServer.Dream.Objects.MetaObjects;
using OpenDreamServer.Dream.Procs;
using OpenDreamShared.Dream;
using System;
using System.Collections.Generic;

namespace OpenDreamServer.Dream.Objects {
    delegate void DreamObjectCreatedDelegate(DreamObject dreamObject);

    class DreamObject {
        public DreamObjectDefinition ObjectDefinition;
        public bool Deleted = false;

        /// <summary>
        /// Any variables that may differ from the default
        /// </summary>
        private readonly Dictionary<string, DreamValue> _variables = new();

        private static readonly Dictionary<DreamObject, int> _referenceIDs = new();

        public DreamObject(DreamObjectDefinition objectDefinition, DreamProcArguments creationArguments) {
            ObjectDefinition = objectDefinition;

            foreach (KeyValuePair<string, (DreamPath, DreamProcArguments)> runtimeInstantiatedVariable in ObjectDefinition.RuntimeInstantiatedVariables) {
                DreamObject instantiatedObject = Program.DreamObjectTree.CreateObject(runtimeInstantiatedVariable.Value.Item1, runtimeInstantiatedVariable.Value.Item2);

                SetVariable(runtimeInstantiatedVariable.Key, new DreamValue(instantiatedObject));
            }

            foreach ((string VariableName, List<(DreamValue, DreamValue)> Values) in ObjectDefinition.RuntimeInstantiatedLists) {
                DreamList list = Program.DreamObjectTree.CreateList();

                foreach ((DreamValue Index, DreamValue Value) value in Values) {
                    if (value.Index.Value != null) {
                        list.SetValue(value.Index, value.Value);
                    } else {
                        list.AddValue(value.Value);
                    }
                }

                SetVariable(VariableName, new DreamValue(list));
            }

            if (ObjectDefinition.MetaObject != null) ObjectDefinition.MetaObject.OnObjectCreated(this, creationArguments);
        }

        ~DreamObject() {
            Delete();
        }

        public static int CreateReferenceID(DreamObject dreamObject) {
            int referenceID;

            if (!_referenceIDs.TryGetValue(dreamObject, out referenceID)) {
                referenceID = _referenceIDs.Count;

                _referenceIDs.Add(dreamObject, referenceID);
            }

            return referenceID;
        }

        public static DreamObject GetFromReferenceID(int refID) {
            foreach (KeyValuePair<DreamObject, int> referenceIDPair in _referenceIDs) {
                if (referenceIDPair.Value == refID) return referenceIDPair.Key;
            }

            return null;
        }

        public void Delete() {
            if (Deleted) return;
            if (ObjectDefinition.MetaObject != null) ObjectDefinition.MetaObject.OnObjectDeleted(this);

            _referenceIDs.Remove(this);
            Deleted = true;
        }

        public bool IsSubtypeOf(DreamPath path) {
            return ObjectDefinition.IsSubtypeOf(path);
        }

        public bool HasVariable(string name) {
            return ObjectDefinition.HasVariable(name); ;
        }

        public bool HasProc(string name) {
            return ObjectDefinition.HasProc(name);
        }

        public DreamValue GetVariable(string name) {
            if (!HasVariable(name)) throw new Exception("Variable '" + name + "' doesn't exist");

            DreamValue variableValue = _variables.ContainsKey(name) ? _variables[name] : ObjectDefinition.Variables[name];
            if (ObjectDefinition.MetaObject != null) {
                return ObjectDefinition.MetaObject.OnVariableGet(this, name, variableValue);
            } else {
                return variableValue;
            }
        }

        public void SetVariable(string name, DreamValue value) {
            if (!HasVariable(name)) throw new Exception("Variable '" + name + "' doesn't exist");

            DreamValue oldValue = _variables.ContainsKey(name) ? _variables[name] : ObjectDefinition.Variables[name];
            _variables[name] = value;
            if (ObjectDefinition.MetaObject != null) ObjectDefinition.MetaObject.OnVariableSet(this, name, value, oldValue);
        }

        public DreamProc GetProc(string procName) {
            return ObjectDefinition.GetProc(procName);
        }

        public DreamValue CallProc(string procName, DreamProcArguments arguments, DreamObject usr = null) {
            try {
                DreamProc proc = GetProc(procName);

                return proc.Run(this, arguments, usr);
            } catch (Exception e) {
                Console.WriteLine("Exception while running proc '" + procName + "' on object of type '" + ObjectDefinition.Type + "': " + e.Message);
            }

            return new DreamValue((DreamObject)null);
        }

        public DreamValue CallProc(string procName) {
            return CallProc(procName, new DreamProcArguments(null));
        }

        public override string ToString() {
            return "DreamObject(" + ObjectDefinition.Type + ")";
        }
    }
}
