﻿using System.Collections.Generic;

namespace DMShared.Json {
    public sealed class DreamCompiledJson {
        public List<string>? Strings { get; set; }
        public string[]? Resources { get; set; }
        public int[]? GlobalProcs { get; set; }
        public GlobalListJson? Globals { get; set; }
        public ProcDefinitionJson? GlobalInitProc { get; set; }
        public List<DreamMapJson>? Maps { get; set; }
        public string? Interface { get; set; }
        public DreamTypeJson[]? Types { get; set; }
        public ProcDefinitionJson[]? Procs { get; set; }
    }
}