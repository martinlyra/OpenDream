using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DMCompiler.DM.Expressions;
using OpenDreamRuntime;
using OpenDreamRuntime.Objects;
using OpenDreamRuntime.Objects.Types;
using OpenDreamRuntime.Procs;
using OpenDreamRuntime.Procs.Native;
using OpenDreamShared.Dream;
using OpenDreamShared.Json;

namespace DMCompiler.DM {
    sealed class DMBuiltins {
        static IDreamObjectTree NativeTree = new BuiltinObjectTree();

        static DMBuiltins() {
            DreamProcNative.SetupNativeProcs(NativeTree);
        }

        public static bool IsNativeProc(string name) => GetNativeProc(name, out _);
        public static bool GetNativeProc(string name, out DreamProc proc) => NativeTree.TryGetGlobalProc(name, out proc);

        public static bool TryEvaluateConstant(GlobalProc globalProc, ArgumentList arguments, out Expressions.Constant? value) {
            DMProc proc = globalProc.GetProc();

            value = null;
            if(!GetNativeProc(proc.Name, out DreamProc found))
                return false;

            NativeProc native = (NativeProc)found;

            // Convert compiler-state to runtime arguments
            List<DreamValue> dreamValues = new List<DreamValue>();
            foreach((string name, DMExpression expression) in arguments.Expressions) {
                Constant constantValue;
                if (!expression.TryAsConstant(out constantValue))
                    return false;

                switch (constantValue) {
                    case Null: {
                        dreamValues.Add(DreamValue.Null);
                        continue;
                    }
                    case Number: {
                        Number num = (Number)constantValue;
                        dreamValues.Add(new DreamValue(num.Value));
                        continue;
                    }
                    case String: {
                        String str = (String)constantValue;
                        dreamValues.Add(new DreamValue(str.Value));
                        continue;
                    }
                    default: return false;
                }
            }

            // Wrap and run the target proc compiletime
            NativeProc.State compilerTimeState = new NativeProc.State();
            compilerTimeState.Initialize(native, null, null, null, new DreamProcArguments(dreamValues.ToArray()));
            compilerTimeState.Resume();
            DreamValue result = compilerTimeState.Result;

            // Unwrap the compiletime result
            switch(result.Type) {
                case DreamValue.DreamValueType.String: {
                    value = new String(proc.Location, result.MustGetValueAsString());
                    return true;
                }
                case DreamValue.DreamValueType.Float: {
                    value = new Number(proc.Location, result.MustGetValueAsFloat());
                    return true;
                }
                default: return false;
            }
        }

        private class BuiltinObjectTree : IDreamObjectTree {
            public IDreamObjectTree.TreeEntry[] Types => throw new System.NotImplementedException();

            public List<DreamProc> Procs {get; private set;}
            private Dictionary<string, int> ProcIdMap = new Dictionary<string, int>();

            public List<string> Strings => throw new System.NotImplementedException();

            public DreamProc? GlobalInitProc => throw new System.NotImplementedException();

            public IDreamObjectTree.TreeEntry Root => throw new System.NotImplementedException();

            public IDreamObjectTree.TreeEntry List {get; private set;} = null;

            public IDreamObjectTree.TreeEntry World {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Client {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Datum {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Sound {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Matrix {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Exception {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Savefile {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Regex {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Filter {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Icon {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Image {get; private set;} = null;

            public IDreamObjectTree.TreeEntry MutableAppearance {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Atom {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Area {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Turf {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Movable {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Obj {get; private set;} = null;

            public IDreamObjectTree.TreeEntry Mob {get; private set;} = null;

            public BuiltinObjectTree() {
                Procs = new List<DreamProc>();
            }

            public DreamList CreateList(int size = 0) {
                throw new System.NotImplementedException();
            }

            public DreamList CreateList(string[] elements) {
                throw new System.NotImplementedException();
            }

            public DreamObject CreateObject(IDreamObjectTree.TreeEntry type) {
                throw new System.NotImplementedException();
            }

            public T CreateObject<T>(IDreamObjectTree.TreeEntry type) where T : DreamObject {
                throw new System.NotImplementedException();
            }

            public IEnumerable<IDreamObjectTree.TreeEntry> GetAllDescendants(IDreamObjectTree.TreeEntry treeEntry) {
                throw new System.NotImplementedException();
            }

            public DreamValue GetDreamValueFromJsonElement(object value) {
                throw new System.NotImplementedException();
            }

            public DreamObjectDefinition GetObjectDefinition(int typeId) {
                throw new System.NotImplementedException();
            }

            public IDreamObjectTree.TreeEntry GetTreeEntry(DreamPath path) {
                throw new System.NotImplementedException();
            }

            public IDreamObjectTree.TreeEntry GetTreeEntry(int typeId) {
                throw new System.NotImplementedException();
            }

            public void LoadJson(DreamCompiledJson json) {
                throw new System.NotImplementedException();
            }

            public void SetGlobalNativeProc(NativeProc.HandlerFn func) {
                var (name, defaultArgumentValues, argumentNames) = NativeProc.GetNativeInfo(func);
                if(!DMObjectTree.GlobalProcs.TryGetValue(name, out _))
                    return;

                var proc = new NativeProc(DreamPath.Root, name, argumentNames, defaultArgumentValues, func, null, null, null, null, this);
                ProcIdMap.Add(name, Procs.Count);
                Procs.Add(proc);
            }

            public void SetGlobalNativeProc(System.Func<AsyncNativeProc.State, Task<DreamValue>> func) {}
            public void SetNativeProc(IDreamObjectTree.TreeEntry type, NativeProc.HandlerFn func) {}
            public void SetNativeProc(IDreamObjectTree.TreeEntry type, System.Func<AsyncNativeProc.State, Task<DreamValue>> func) {}

            public bool TryGetGlobalProc(string name, [NotNullWhen(true)] out DreamProc? globalProc) {
                globalProc = ProcIdMap.TryGetValue(name, out int procId) ? Procs[procId] : null;

                return (globalProc != null);
            }

            public bool TryGetTreeEntry(DreamPath path, [NotNullWhen(true)] out IDreamObjectTree.TreeEntry? treeEntry) {
                throw new System.NotImplementedException();
            }
        }
    }
}
