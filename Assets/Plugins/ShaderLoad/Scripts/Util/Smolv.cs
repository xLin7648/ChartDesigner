// smol-v — public domain — https://github.com/aras-p/smol-v
// authored 2016-2024 by Aras Pranckevicius
//
// Pure C# translation of the smol-v SPIR-V compression library.
// No native dependencies — works everywhere Unity runs (including IL2CPP, WebGL, consoles).
//
// Licensed under MIT or Public Domain (same as the original library).

using System;
using System.Collections.Generic;

namespace ShaderLoad.Util
{
    public static class Smolv
    {
        // --------------------------------------------------------------------------------------------
        // SPIR-V opcodes
        // --------------------------------------------------------------------------------------------

        enum SpvOp
        {
            Nop = 0,
            Undef = 1,
            SourceContinued = 2,
            Source = 3,
            SourceExtension = 4,
            Name = 5,
            MemberName = 6,
            String = 7,
            Line = 8,
            Extension = 10,
            ExtInstImport = 11,
            ExtInst = 12,
            VectorShuffleCompact = 13,
            MemoryModel = 14,
            EntryPoint = 15,
            ExecutionMode = 16,
            Capability = 17,
            TypeVoid = 19,
            TypeBool = 20,
            TypeInt = 21,
            TypeFloat = 22,
            TypeVector = 23,
            TypeMatrix = 24,
            TypeImage = 25,
            TypeSampler = 26,
            TypeSampledImage = 27,
            TypeArray = 28,
            TypeRuntimeArray = 29,
            TypeStruct = 30,
            TypeOpaque = 31,
            TypePointer = 32,
            TypeFunction = 33,
            TypeEvent = 34,
            TypeDeviceEvent = 35,
            TypeReserveId = 36,
            TypeQueue = 37,
            TypePipe = 38,
            TypeForwardPointer = 39,
            ConstantTrue = 41,
            ConstantFalse = 42,
            Constant = 43,
            ConstantComposite = 44,
            ConstantSampler = 45,
            ConstantNull = 46,
            SpecConstantTrue = 48,
            SpecConstantFalse = 49,
            SpecConstant = 50,
            SpecConstantComposite = 51,
            SpecConstantOp = 52,
            Function = 54,
            FunctionParameter = 55,
            FunctionEnd = 56,
            FunctionCall = 57,
            Variable = 59,
            ImageTexelPointer = 60,
            Load = 61,
            Store = 62,
            CopyMemory = 63,
            CopyMemorySized = 64,
            AccessChain = 65,
            InBoundsAccessChain = 66,
            PtrAccessChain = 67,
            ArrayLength = 68,
            GenericPtrMemSemantics = 69,
            InBoundsPtrAccessChain = 70,
            Decorate = 71,
            MemberDecorate = 72,
            DecorationGroup = 73,
            GroupDecorate = 74,
            GroupMemberDecorate = 75,
            VectorExtractDynamic = 77,
            VectorInsertDynamic = 78,
            VectorShuffle = 79,
            CompositeConstruct = 80,
            CompositeExtract = 81,
            CompositeInsert = 82,
            CopyObject = 83,
            Transpose = 84,
            SampledImage = 86,
            ImageSampleImplicitLod = 87,
            ImageSampleExplicitLod = 88,
            ImageSampleDrefImplicitLod = 89,
            ImageSampleDrefExplicitLod = 90,
            ImageSampleProjImplicitLod = 91,
            ImageSampleProjExplicitLod = 92,
            ImageSampleProjDrefImplicitLod = 93,
            ImageSampleProjDrefExplicitLod = 94,
            ImageFetch = 95,
            ImageGather = 96,
            ImageDrefGather = 97,
            ImageRead = 98,
            ImageWrite = 99,
            Image = 100,
            ImageQueryFormat = 101,
            ImageQueryOrder = 102,
            ImageQuerySizeLod = 103,
            ImageQuerySize = 104,
            ImageQueryLod = 105,
            ImageQueryLevels = 106,
            ImageQuerySamples = 107,
            ConvertFToU = 109,
            ConvertFToS = 110,
            ConvertSToF = 111,
            ConvertUToF = 112,
            UConvert = 113,
            SConvert = 114,
            FConvert = 115,
            QuantizeToF16 = 116,
            ConvertPtrToU = 117,
            SatConvertSToU = 118,
            SatConvertUToS = 119,
            ConvertUToPtr = 120,
            PtrCastToGeneric = 121,
            GenericCastToPtr = 122,
            GenericCastToPtrExplicit = 123,
            Bitcast = 124,
            SNegate = 126,
            FNegate = 127,
            IAdd = 128,
            FAdd = 129,
            ISub = 130,
            FSub = 131,
            IMul = 132,
            FMul = 133,
            UDiv = 134,
            SDiv = 135,
            FDiv = 136,
            UMod = 137,
            SRem = 138,
            SMod = 139,
            FRem = 140,
            FMod = 141,
            VectorTimesScalar = 142,
            MatrixTimesScalar = 143,
            VectorTimesMatrix = 144,
            MatrixTimesVector = 145,
            MatrixTimesMatrix = 146,
            OuterProduct = 147,
            Dot = 148,
            IAddCarry = 149,
            ISubBorrow = 150,
            UMulExtended = 151,
            SMulExtended = 152,
            Any = 154,
            All = 155,
            IsNan = 156,
            IsInf = 157,
            IsFinite = 158,
            IsNormal = 159,
            SignBitSet = 160,
            LessOrGreater = 161,
            Ordered = 162,
            Unordered = 163,
            LogicalEqual = 164,
            LogicalNotEqual = 165,
            LogicalOr = 166,
            LogicalAnd = 167,
            LogicalNot = 168,
            Select = 169,
            IEqual = 170,
            INotEqual = 171,
            UGreaterThan = 172,
            SGreaterThan = 173,
            UGreaterThanEqual = 174,
            SGreaterThanEqual = 175,
            ULessThan = 176,
            SLessThan = 177,
            ULessThanEqual = 178,
            SLessThanEqual = 179,
            FOrdEqual = 180,
            FUnordEqual = 181,
            FOrdNotEqual = 182,
            FUnordNotEqual = 183,
            FOrdLessThan = 184,
            FUnordLessThan = 185,
            FOrdGreaterThan = 186,
            FUnordGreaterThan = 187,
            FOrdLessThanEqual = 188,
            FUnordLessThanEqual = 189,
            FOrdGreaterThanEqual = 190,
            FUnordGreaterThanEqual = 191,
            ShiftRightLogical = 194,
            ShiftRightArithmetic = 195,
            ShiftLeftLogical = 196,
            BitwiseOr = 197,
            BitwiseXor = 198,
            BitwiseAnd = 199,
            Not = 200,
            BitFieldInsert = 201,
            BitFieldSExtract = 202,
            BitFieldUExtract = 203,
            BitReverse = 204,
            BitCount = 205,
            DPdx = 207,
            DPdy = 208,
            Fwidth = 209,
            DPdxFine = 210,
            DPdyFine = 211,
            FwidthFine = 212,
            DPdxCoarse = 213,
            DPdyCoarse = 214,
            FwidthCoarse = 215,
            EmitVertex = 218,
            EndPrimitive = 219,
            EmitStreamVertex = 220,
            EndStreamPrimitive = 221,
            ControlBarrier = 224,
            MemoryBarrier = 225,
            AtomicLoad = 227,
            AtomicStore = 228,
            AtomicExchange = 229,
            AtomicCompareExchange = 230,
            AtomicCompareExchangeWeak = 231,
            AtomicIIncrement = 232,
            AtomicIDecrement = 233,
            AtomicIAdd = 234,
            AtomicISub = 235,
            AtomicSMin = 236,
            AtomicUMin = 237,
            AtomicSMax = 238,
            AtomicUMax = 239,
            AtomicAnd = 240,
            AtomicOr = 241,
            AtomicXor = 242,
            Phi = 245,
            LoopMerge = 246,
            SelectionMerge = 247,
            Label = 248,
            Branch = 249,
            BranchConditional = 250,
            Switch = 251,
            Kill = 252,
            Return = 253,
            ReturnValue = 254,
            Unreachable = 255,
            LifetimeStart = 256,
            LifetimeStop = 257,
            GroupAsyncCopy = 259,
            GroupWaitEvents = 260,
            GroupAll = 261,
            GroupAny = 262,
            GroupBroadcast = 263,
            GroupIAdd = 264,
            GroupFAdd = 265,
            GroupFMin = 266,
            GroupUMin = 267,
            GroupSMin = 268,
            GroupFMax = 269,
            GroupUMax = 270,
            GroupSMax = 271,
            ReadPipe = 274,
            WritePipe = 275,
            ReservedReadPipe = 276,
            ReservedWritePipe = 277,
            ReserveReadPipePackets = 278,
            ReserveWritePipePackets = 279,
            CommitReadPipe = 280,
            CommitWritePipe = 281,
            IsValidReserveId = 282,
            GetNumPipePackets = 283,
            GetMaxPipePackets = 284,
            GroupReserveReadPipePackets = 285,
            GroupReserveWritePipePackets = 286,
            GroupCommitReadPipe = 287,
            GroupCommitWritePipe = 288,
            EnqueueMarker = 291,
            EnqueueKernel = 292,
            GetKernelNDrangeSubGroupCount = 293,
            GetKernelNDrangeMaxSubGroupSize = 294,
            GetKernelWorkGroupSize = 295,
            GetKernelPreferredWorkGroupSizeMultiple = 296,
            RetainEvent = 297,
            ReleaseEvent = 298,
            CreateUserEvent = 299,
            IsValidEvent = 300,
            SetUserEventStatus = 301,
            CaptureEventProfilingInfo = 302,
            GetDefaultQueue = 303,
            BuildNDRange = 304,
            ImageSparseSampleImplicitLod = 305,
            ImageSparseSampleExplicitLod = 306,
            ImageSparseSampleDrefImplicitLod = 307,
            ImageSparseSampleDrefExplicitLod = 308,
            ImageSparseSampleProjImplicitLod = 309,
            ImageSparseSampleProjExplicitLod = 310,
            ImageSparseSampleProjDrefImplicitLod = 311,
            ImageSparseSampleProjDrefExplicitLod = 312,
            ImageSparseFetch = 313,
            ImageSparseGather = 314,
            ImageSparseDrefGather = 315,
            ImageSparseTexelsResident = 316,
            NoLine = 317,
            AtomicFlagTestAndSet = 318,
            AtomicFlagClear = 319,
            ImageSparseRead = 320,
            SizeOf = 321,
            TypePipeStorage = 322,
            ConstantPipeStorage = 323,
            CreatePipeFromPipeStorage = 324,
            GetKernelLocalSizeForSubgroupCount = 325,
            GetKernelMaxNumSubgroups = 326,
            TypeNamedBarrier = 327,
            NamedBarrierInitialize = 328,
            MemoryNamedBarrier = 329,
            ModuleProcessed = 330,
            ExecutionModeId = 331,
            DecorateId = 332,
            GroupNonUniformElect = 333,
            GroupNonUniformAll = 334,
            GroupNonUniformAny = 335,
            GroupNonUniformAllEqual = 336,
            GroupNonUniformBroadcast = 337,
            GroupNonUniformBroadcastFirst = 338,
            GroupNonUniformBallot = 339,
            GroupNonUniformInverseBallot = 340,
            GroupNonUniformBallotBitExtract = 341,
            GroupNonUniformBallotBitCount = 342,
            GroupNonUniformBallotFindLSB = 343,
            GroupNonUniformBallotFindMSB = 344,
            GroupNonUniformShuffle = 345,
            GroupNonUniformShuffleXor = 346,
            GroupNonUniformShuffleUp = 347,
            GroupNonUniformShuffleDown = 348,
            GroupNonUniformIAdd = 349,
            GroupNonUniformFAdd = 350,
            GroupNonUniformIMul = 351,
            GroupNonUniformFMul = 352,
            GroupNonUniformSMin = 353,
            GroupNonUniformUMin = 354,
            GroupNonUniformFMin = 355,
            GroupNonUniformSMax = 356,
            GroupNonUniformUMax = 357,
            GroupNonUniformFMax = 358,
            GroupNonUniformBitwiseAnd = 359,
            GroupNonUniformBitwiseOr = 360,
            GroupNonUniformBitwiseXor = 361,
            GroupNonUniformLogicalAnd = 362,
            GroupNonUniformLogicalOr = 363,
            GroupNonUniformLogicalXor = 364,
            GroupNonUniformQuadBroadcast = 365,
            GroupNonUniformQuadSwap = 366,
        }
        const int kKnownOpsCount = (int)SpvOp.GroupNonUniformQuadSwap + 1;

        // --------------------------------------------------------------------------------------------
        // Opcode metadata
        // --------------------------------------------------------------------------------------------

        struct OpData
        {
            public byte hasResult;
            public byte hasType;
            public byte deltaFromResult;
            public byte varrest;
        }

        static readonly OpData[] kSpirvOpData = new OpData[kKnownOpsCount]
        {
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // Nop
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // Undef
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // SourceContinued
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=1}, // Source
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // SourceExtension
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // Name
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // MemberName
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // String
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=1}, // Line
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #9
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // Extension
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=0}, // ExtInstImport
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=1}, // ExtInst
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // VectorShuffleCompact
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=1}, // MemoryModel
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=1}, // EntryPoint
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=1}, // ExecutionMode
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=1}, // Capability
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #18
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeVoid
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeBool
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeInt
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeFloat
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeVector
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeMatrix
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeImage
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeSampler
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeSampledImage
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeArray
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeRuntimeArray
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeStruct
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeOpaque
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypePointer
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeFunction
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeEvent
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeDeviceEvent
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeReserveId
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypeQueue
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=1}, // TypePipe
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=1}, // TypeForwardPointer
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #40
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ConstantTrue
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ConstantFalse
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // Constant
        new OpData{hasResult=1,hasType=1,deltaFromResult=9,varrest=0}, // ConstantComposite
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=1}, // ConstantSampler
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ConstantNull
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #47
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // SpecConstantTrue
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // SpecConstantFalse
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // SpecConstant
        new OpData{hasResult=1,hasType=1,deltaFromResult=9,varrest=0}, // SpecConstantComposite
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // SpecConstantOp
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #53
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=1}, // Function
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // FunctionParameter
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // FunctionEnd
        new OpData{hasResult=1,hasType=1,deltaFromResult=9,varrest=0}, // FunctionCall
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #58
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=1}, // Variable
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ImageTexelPointer
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // Load
        new OpData{hasResult=0,hasType=0,deltaFromResult=2,varrest=1}, // Store
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // CopyMemory
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // CopyMemorySized
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=1}, // AccessChain
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // InBoundsAccessChain
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // PtrAccessChain
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ArrayLength
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GenericPtrMemSemantics
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // InBoundsPtrAccessChain
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=1}, // Decorate
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=1}, // MemberDecorate
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=0}, // DecorationGroup
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // GroupDecorate
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // GroupMemberDecorate
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #76
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // VectorExtractDynamic
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // VectorInsertDynamic
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // VectorShuffle
        new OpData{hasResult=1,hasType=1,deltaFromResult=9,varrest=0}, // CompositeConstruct
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // CompositeExtract
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // CompositeInsert
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // CopyObject
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // Transpose
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #85
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // SampledImage
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // ImageSampleImplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // ImageSampleExplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageSampleDrefImplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageSampleDrefExplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // ImageSampleProjImplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // ImageSampleProjExplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageSampleProjDrefImplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageSampleProjDrefExplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // ImageFetch
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageGather
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageDrefGather
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // ImageRead
        new OpData{hasResult=0,hasType=0,deltaFromResult=3,varrest=1}, // ImageWrite
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // Image
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ImageQueryFormat
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ImageQueryOrder
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // ImageQuerySizeLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ImageQuerySize
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // ImageQueryLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ImageQueryLevels
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ImageQuerySamples
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #108
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ConvertFToU
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ConvertFToS
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ConvertSToF
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ConvertUToF
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // UConvert
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // SConvert
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // FConvert
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // QuantizeToF16
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ConvertPtrToU
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // SatConvertSToU
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // SatConvertUToS
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ConvertUToPtr
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // PtrCastToGeneric
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // GenericCastToPtr
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GenericCastToPtrExplicit
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // Bitcast
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #125
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // SNegate
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // FNegate
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // IAdd
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FAdd
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // ISub
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FSub
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // IMul
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FMul
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // UDiv
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // SDiv
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FDiv
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // UMod
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // SRem
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // SMod
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FRem
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FMod
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // VectorTimesScalar
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // MatrixTimesScalar
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // VectorTimesMatrix
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // MatrixTimesVector
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // MatrixTimesMatrix
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // OuterProduct
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // Dot
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // IAddCarry
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // ISubBorrow
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // UMulExtended
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // SMulExtended
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #153
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // Any
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // All
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // IsNan
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // IsInf
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // IsFinite
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // IsNormal
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // SignBitSet
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // LessOrGreater
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // Ordered
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // Unordered
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // LogicalEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // LogicalNotEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // LogicalOr
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // LogicalAnd
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // LogicalNot
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=0}, // Select
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // IEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // INotEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // UGreaterThan
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // SGreaterThan
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // UGreaterThanEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // SGreaterThanEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // ULessThan
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // SLessThan
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // ULessThanEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // SLessThanEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FOrdEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FUnordEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FOrdNotEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FUnordNotEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FOrdLessThan
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FUnordLessThan
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FOrdGreaterThan
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FUnordGreaterThan
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FOrdLessThanEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FUnordLessThanEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FOrdGreaterThanEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // FUnordGreaterThanEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #192
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #193
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // ShiftRightLogical
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // ShiftRightArithmetic
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // ShiftLeftLogical
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // BitwiseOr
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // BitwiseXor
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=0}, // BitwiseAnd
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // Not
        new OpData{hasResult=1,hasType=1,deltaFromResult=4,varrest=0}, // BitFieldInsert
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=0}, // BitFieldSExtract
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=0}, // BitFieldUExtract
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // BitReverse
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // BitCount
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #206
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // DPdx
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // DPdy
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // Fwidth
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // DPdxFine
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // DPdyFine
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // FwidthFine
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // DPdxCoarse
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // DPdyCoarse
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // FwidthCoarse
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #216
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #217
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // EmitVertex
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // EndPrimitive
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // EmitStreamVertex
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // EndStreamPrimitive
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #222
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #223
        new OpData{hasResult=0,hasType=0,deltaFromResult=3,varrest=0}, // ControlBarrier
        new OpData{hasResult=0,hasType=0,deltaFromResult=2,varrest=0}, // MemoryBarrier
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #226
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicLoad
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // AtomicStore
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicExchange
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicCompareExchange
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicCompareExchangeWeak
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicIIncrement
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicIDecrement
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicIAdd
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicISub
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicSMin
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicUMin
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicSMax
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicUMax
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicAnd
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicOr
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicXor
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #243
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #244
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // Phi
        new OpData{hasResult=0,hasType=0,deltaFromResult=2,varrest=1}, // LoopMerge
        new OpData{hasResult=0,hasType=0,deltaFromResult=1,varrest=1}, // SelectionMerge
        new OpData{hasResult=1,hasType=0,deltaFromResult=0,varrest=0}, // Label
        new OpData{hasResult=0,hasType=0,deltaFromResult=1,varrest=0}, // Branch
        new OpData{hasResult=0,hasType=0,deltaFromResult=3,varrest=1}, // BranchConditional
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // Switch
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // Kill
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // Return
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // ReturnValue
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // Unreachable
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // LifetimeStart
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // LifetimeStop
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #258
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupAsyncCopy
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // GroupWaitEvents
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupAll
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupAny
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupBroadcast
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupIAdd
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupFAdd
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupFMin
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupUMin
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupSMin
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupFMax
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupUMax
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupSMax
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #272
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #273
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ReadPipe
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // WritePipe
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ReservedReadPipe
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ReservedWritePipe
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ReserveReadPipePackets
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ReserveWritePipePackets
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // CommitReadPipe
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // CommitWritePipe
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // IsValidReserveId
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GetNumPipePackets
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GetMaxPipePackets
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupReserveReadPipePackets
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GroupReserveWritePipePackets
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // GroupCommitReadPipe
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // GroupCommitWritePipe
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #289
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // #290
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // EnqueueMarker
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // EnqueueKernel
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GetKernelNDrangeSubGroupCount
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GetKernelNDrangeMaxSubGroupSize
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GetKernelWorkGroupSize
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GetKernelPreferredWorkGroupSizeMultiple
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // RetainEvent
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // ReleaseEvent
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // CreateUserEvent
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // IsValidEvent
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // SetUserEventStatus
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // CaptureEventProfilingInfo
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GetDefaultQueue
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // BuildNDRange
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // ImageSparseSampleImplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // ImageSparseSampleExplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageSparseSampleDrefImplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageSparseSampleDrefExplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // ImageSparseSampleProjImplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // ImageSparseSampleProjExplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageSparseSampleProjDrefImplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageSparseSampleProjDrefExplicitLod
        new OpData{hasResult=1,hasType=1,deltaFromResult=2,varrest=1}, // ImageSparseFetch
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageSparseGather
        new OpData{hasResult=1,hasType=1,deltaFromResult=3,varrest=1}, // ImageSparseDrefGather
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=0}, // ImageSparseTexelsResident
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // NoLine
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // AtomicFlagTestAndSet
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=0}, // AtomicFlagClear
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ImageSparseRead
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // SizeOf
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // TypePipeStorage
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ConstantPipeStorage
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // CreatePipeFromPipeStorage
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GetKernelLocalSizeForSubgroupCount
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // GetKernelMaxNumSubgroups
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // TypeNamedBarrier
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=1}, // NamedBarrierInitialize
        new OpData{hasResult=0,hasType=0,deltaFromResult=2,varrest=1}, // MemoryNamedBarrier
        new OpData{hasResult=1,hasType=1,deltaFromResult=0,varrest=0}, // ModuleProcessed
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=1}, // ExecutionModeId
        new OpData{hasResult=0,hasType=0,deltaFromResult=0,varrest=1}, // DecorateId
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformElect
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformAll
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformAny
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformAllEqual
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformBroadcast
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformBroadcastFirst
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformBallot
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformInverseBallot
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformBallotBitExtract
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformBallotBitCount
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformBallotFindLSB
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformBallotFindMSB
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformShuffle
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformShuffleXor
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformShuffleUp
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformShuffleDown
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformIAdd
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformFAdd
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformIMul
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformFMul
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformSMin
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformUMin
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformFMin
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformSMax
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformUMax
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformFMax
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformBitwiseAnd
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformBitwiseOr
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformBitwiseXor
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformLogicalAnd
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformLogicalOr
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformLogicalXor
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformQuadBroadcast
        new OpData{hasResult=1,hasType=1,deltaFromResult=1,varrest=1}, // GroupNonUniformQuadSwap
        };

        // --------------------------------------------------------------------------------------------
        // Helper functions for opcode metadata queries
        // --------------------------------------------------------------------------------------------

        static int GetKnownOpsCount(int version)
        {
            if (version == 0)
                return (int)SpvOp.ModuleProcessed + 1;
            if (version == 1)
                return (int)SpvOp.GroupNonUniformQuadSwap + 1;
            return 0;
        }

        static bool OpHasResult(SpvOp op, int opsCount)
        {
            int idx = (int)op;
            if (idx < 0 || idx >= opsCount) return false;
            return kSpirvOpData[idx].hasResult != 0;
        }

        static bool OpHasType(SpvOp op, int opsCount)
        {
            int idx = (int)op;
            if (idx < 0 || idx >= opsCount) return false;
            return kSpirvOpData[idx].hasType != 0;
        }

        static int OpDeltaFromResult(SpvOp op, int opsCount)
        {
            int idx = (int)op;
            if (idx < 0 || idx >= opsCount) return 0;
            return kSpirvOpData[idx].deltaFromResult;
        }

        static bool OpVarRest(SpvOp op, int opsCount)
        {
            int idx = (int)op;
            if (idx < 0 || idx >= opsCount) return false;
            return kSpirvOpData[idx].varrest != 0;
        }

        static bool OpDebugInfo(SpvOp op)
        {
            return op == SpvOp.SourceContinued ||
                   op == SpvOp.Source ||
                   op == SpvOp.SourceExtension ||
                   op == SpvOp.Name ||
                   op == SpvOp.MemberName ||
                   op == SpvOp.String ||
                   op == SpvOp.Line ||
                   op == SpvOp.NoLine ||
                   op == SpvOp.ModuleProcessed;
        }

        static int DecorationExtraOps(int dec)
        {
            if (dec == 0 || (dec >= 2 && dec <= 5))
                return 0;
            if (dec >= 29 && dec <= 37)
                return 1;
            return -1;
        }

        // --------------------------------------------------------------------------------------------
        // Constants
        // --------------------------------------------------------------------------------------------

        const uint kSpirVHeaderMagic = 0x07230203u;
        const uint kSmolHeaderMagic = 0x534D4F4Cu; // "SMOL"
        const int kSmolCurrEncodingVersion = 1;

        // --------------------------------------------------------------------------------------------
        // Header validation
        // --------------------------------------------------------------------------------------------

        static bool CheckGenericHeader(uint[] words, int wordCount, uint expectedMagic, uint versionMask)
        {
            if (wordCount < 5) return false;
            if (words[0] != expectedMagic) return false;
            uint headerVersion = words[1] & versionMask;
            if (headerVersion < 0x00010000 || headerVersion > 0x00010600)
                return false;
            return true;
        }

        static bool CheckSpirVHeader(uint[] words, int wordCount)
        {
            return CheckGenericHeader(words, wordCount, kSpirVHeaderMagic, 0xFFFFFFFFu);
        }

        static bool CheckSmolHeader(byte[] bytes, int byteCount)
        {
            if (byteCount < 24) return false;
            uint[] words = new uint[6];
            Buffer.BlockCopy(bytes, 0, words, 0, 24);
            if (!CheckGenericHeader(words, byteCount / 4, kSmolHeaderMagic, 0x00FFFFFFu))
                return false;
            int smolVersion = (int)(words[1] >> 24);
            if (smolVersion < 0 || smolVersion > kSmolCurrEncodingVersion)
                return false;
            return true;
        }

        // --------------------------------------------------------------------------------------------
        // I/O primitives
        // --------------------------------------------------------------------------------------------

        static void Write4(List<byte> buf, uint v)
        {
            buf.Add((byte)(v & 0xFF));
            buf.Add((byte)((v >> 8) & 0xFF));
            buf.Add((byte)((v >> 16) & 0xFF));
            buf.Add((byte)(v >> 24));
        }

        static void Write4(byte[] buf, ref int pos, uint v)
        {
            buf[pos++] = (byte)(v & 0xFF);
            buf[pos++] = (byte)((v >> 8) & 0xFF);
            buf[pos++] = (byte)((v >> 16) & 0xFF);
            buf[pos++] = (byte)(v >> 24);
        }

        static bool Read4(byte[] data, ref int pos, int end, out uint val)
        {
            if (pos + 4 > end)
            {
                val = 0;
                return false;
            }
            val = (uint)data[pos] |
                  ((uint)data[pos + 1] << 8) |
                  ((uint)data[pos + 2] << 16) |
                  ((uint)data[pos + 3] << 24);
            pos += 4;
            return true;
        }

        static void WriteVarint(List<byte> buf, uint v)
        {
            while (v > 127)
            {
                buf.Add((byte)((v & 127) | 128));
                v >>= 7;
            }
            buf.Add((byte)(v & 127));
        }

        static bool ReadVarint(byte[] data, ref int pos, int end, out uint val)
        {
            uint v = 0;
            int shift = 0;
            while (pos < end)
            {
                byte b = data[pos++];
                v |= (uint)(b & 127) << shift;
                shift += 7;
                if ((b & 128) == 0)
                    break;
            }
            val = v;
            return true;
        }

        static uint ZigEncode(int i)
        {
            return ((uint)i << 1) ^ (uint)(i >> 31);
        }

        static int ZigDecode(uint u)
        {
            return (u & 1) != 0 ? (int)(~(u >> 1)) : (int)(u >> 1);
        }

        // --------------------------------------------------------------------------------------------
        // Opcode remapping
        // --------------------------------------------------------------------------------------------

        static SpvOp RemapOp(SpvOp op)
        {
            if (op == SpvOp.Decorate) return SpvOp.Nop;
            if (op == SpvOp.Nop) return SpvOp.Decorate;

            if (op == SpvOp.Load) return SpvOp.Undef;
            if (op == SpvOp.Undef) return SpvOp.Load;

            if (op == SpvOp.Store) return SpvOp.SourceContinued;
            if (op == SpvOp.SourceContinued) return SpvOp.Store;

            if (op == SpvOp.AccessChain) return SpvOp.Source;
            if (op == SpvOp.Source) return SpvOp.AccessChain;

            if (op == SpvOp.VectorShuffle) return SpvOp.SourceExtension;
            if (op == SpvOp.SourceExtension) return SpvOp.VectorShuffle;

            if (op == SpvOp.MemberDecorate) return SpvOp.String;
            if (op == SpvOp.String) return SpvOp.MemberDecorate;

            if (op == SpvOp.Label) return SpvOp.Line;
            if (op == SpvOp.Line) return SpvOp.Label;

            if (op == SpvOp.Variable) return (SpvOp)9;
            if ((int)op == 9) return SpvOp.Variable;

            if (op == SpvOp.FMul) return SpvOp.Extension;
            if (op == SpvOp.Extension) return SpvOp.FMul;

            if (op == SpvOp.FAdd) return SpvOp.ExtInstImport;
            if (op == SpvOp.ExtInstImport) return SpvOp.FAdd;

            if (op == SpvOp.TypePointer) return SpvOp.MemoryModel;
            if (op == SpvOp.MemoryModel) return SpvOp.TypePointer;

            if (op == SpvOp.FNegate) return SpvOp.EntryPoint;
            if (op == SpvOp.EntryPoint) return SpvOp.FNegate;

            return op;
        }

        static uint EncodeLen(SpvOp op, uint len)
        {
            len--;
            if (op == SpvOp.VectorShuffle) len -= 4;
            if (op == SpvOp.VectorShuffleCompact) len -= 4;
            if (op == SpvOp.Decorate) len -= 2;
            if (op == SpvOp.Load) len -= 3;
            if (op == SpvOp.AccessChain) len -= 3;
            return len;
        }

        static uint DecodeLen(SpvOp op, uint len)
        {
            len++;
            if (op == SpvOp.VectorShuffle) len += 4;
            if (op == SpvOp.VectorShuffleCompact) len += 4;
            if (op == SpvOp.Decorate) len += 2;
            if (op == SpvOp.Load) len += 3;
            if (op == SpvOp.AccessChain) len += 3;
            return len;
        }

        static bool WriteLengthOp(List<byte> buf, uint len, SpvOp op)
        {
            len = EncodeLen(op, len);
            if (len > 0xFFFF)
                return false;
            op = RemapOp(op);
            uint oplen = ((len >> 4) << 20) | (((uint)op >> 4) << 8) | ((len & 0xF) << 4) | ((uint)op & 0xF);
            WriteVarint(buf, oplen);
            return true;
        }

        static bool ReadLengthOp(byte[] data, ref int pos, int end, out uint outLen, out SpvOp outOp)
        {
            uint val;
            if (!ReadVarint(data, ref pos, end, out val))
            {
                outLen = 0;
                outOp = 0;
                return false;
            }
            outLen = ((val >> 20) << 4) | ((val >> 4) & 0xF);
            outOp = (SpvOp)((int)(((val >> 4) & 0xFFF0) | (val & 0xF)));

            outOp = RemapOp(outOp);
            outLen = DecodeLen(outOp, outLen);
            return true;
        }

        // --------------------------------------------------------------------------------------------
        // Public API — Encode
        // --------------------------------------------------------------------------------------------

        [Flags]
        public enum EncodeFlags
        {
            None = 0,
            StripDebugInfo = 1 << 0,
        }

        [Flags]
        public enum DecodeFlags
        {
            None = 0,
            Use20160831AsZeroVersion = 1 << 0,
        }

        /// <summary>
        /// Encode SPIR-V into SMOL-V.
        /// </summary>
        /// <param name="spirv">Raw SPIR-V binary (length must be multiple of 4).</param>
        /// <param name="flags">Optional encoding flags.</param>
        /// <param name="stripFilter">Optional callback; return true to preserve OpName, false to strip.</param>
        /// <returns>Compressed SMOL-V data, or null on failure.</returns>
        public static byte[] Encode(byte[] spirv, EncodeFlags flags = EncodeFlags.None, Func<string, bool> stripFilter = null)
        {
            if (spirv == null || spirv.Length == 0 || spirv.Length % 4 != 0)
                return null;

            int wordCount = spirv.Length / 4;
            uint[] words = new uint[wordCount];
            Buffer.BlockCopy(spirv, 0, words, 0, spirv.Length);

            if (!CheckSpirVHeader(words, wordCount))
                return null;

            var outSmolv = new List<byte>(spirv.Length / 2);

            // Header
            Write4(outSmolv, kSmolHeaderMagic);
            Write4(outSmolv, (words[1] & 0x00FFFFFFu) + ((uint)kSmolCurrEncodingVersion << 24));
            Write4(outSmolv, words[2]); // generator
            Write4(outSmolv, words[3]); // bound
            Write4(outSmolv, words[4]); // schema

            int headerSpirvSizeOffset = outSmolv.Count;
            Write4(outSmolv, (uint)spirv.Length);

            int strippedSpirvWordCount = wordCount;
            uint prevResult = 0;
            uint prevDecorate = 0;

            int knownOpsCount = GetKnownOpsCount(kSmolCurrEncodingVersion);

            int wordIdx = 5; // skip header
            while (wordIdx < wordCount)
            {
                uint instrLen = words[wordIdx] >> 16;
                if (instrLen < 1) return null;
                if (wordIdx + (int)instrLen > wordCount) return null;
                SpvOp op = (SpvOp)(words[wordIdx] & 0xFFFF);

                // Strip debug info?
                if ((flags & EncodeFlags.StripDebugInfo) != 0 && OpDebugInfo(op))
                {
                    bool strip = true;
                    if (stripFilter != null && op == SpvOp.Name)
                    {
                        // OpName: words[2] is the target ID, words[3].. is the name string
                        string name = ReadStringFromWords(words, wordIdx + 2, (int)instrLen - 1);
                        strip = !stripFilter(name);
                    }
                    if (strip)
                    {
                        strippedSpirvWordCount -= (int)instrLen;
                        wordIdx += (int)instrLen;
                        continue;
                    }
                }

                // Compact VectorShuffle
                uint swizzle = 0;
                if (op == SpvOp.VectorShuffle && instrLen <= 9)
                {
                    uint swz0 = instrLen > 5 ? words[wordIdx + 5] : 0;
                    uint swz1 = instrLen > 6 ? words[wordIdx + 6] : 0;
                    uint swz2 = instrLen > 7 ? words[wordIdx + 7] : 0;
                    uint swz3 = instrLen > 8 ? words[wordIdx + 8] : 0;
                    if (swz0 < 4 && swz1 < 4 && swz2 < 4 && swz3 < 4)
                    {
                        op = SpvOp.VectorShuffleCompact;
                        swizzle = (swz0 << 6) | (swz1 << 4) | (swz2 << 2) | swz3;
                    }
                }

                // Length + opcode
                if (!WriteLengthOp(outSmolv, instrLen, op))
                    return null;

                int ioffs = 1;

                // Type as varint
                if (OpHasType(op, knownOpsCount))
                {
                    if (ioffs >= instrLen) return null;
                    WriteVarint(outSmolv, words[wordIdx + ioffs]);
                    ioffs++;
                }

                // Result as delta+zig+varint
                if (OpHasResult(op, knownOpsCount))
                {
                    if (ioffs >= instrLen) return null;
                    uint v = words[wordIdx + ioffs];
                    WriteVarint(outSmolv, ZigEncode((int)(v - prevResult)));
                    prevResult = v;
                    ioffs++;
                }

                // Decorate/MemberDecorate: IDs relative to previous
                if (op == SpvOp.Decorate || op == SpvOp.MemberDecorate)
                {
                    if (ioffs >= instrLen) return null;
                    uint v = words[wordIdx + ioffs];
                    WriteVarint(outSmolv, ZigEncode((int)(v - prevDecorate)));
                    prevDecorate = v;
                    ioffs++;
                }

                // MemberDecorate batch encoding
                if (op == SpvOp.MemberDecorate)
                {
                    uint decorationType = words[wordIdx + ioffs - 1];
                    int memberWordIdx = wordIdx;
                    uint prevIndex = 0;
                    uint prevOffset = 0;

                    int countLocation = outSmolv.Count;
                    outSmolv.Add(0);
                    int count = 0;

                    while (memberWordIdx < wordCount && count < 255)
                    {
                        uint memberLen = words[memberWordIdx] >> 16;
                        if (memberLen < 1) return null;
                        if (memberWordIdx + (int)memberLen > wordCount) return null;
                        SpvOp memberOp = (SpvOp)(words[memberWordIdx] & 0xFFFF);

                        if (memberOp != SpvOp.MemberDecorate)
                            break;
                        if (memberLen < 4) return null;
                        if (words[memberWordIdx + 1] != decorationType)
                            break;

                        uint memberIndex = words[memberWordIdx + 2];
                        WriteVarint(outSmolv, memberIndex - prevIndex);
                        prevIndex = memberIndex;

                        uint memberDec = words[memberWordIdx + 3];
                        WriteVarint(outSmolv, memberDec);
                        int knownExtraOps = DecorationExtraOps((int)memberDec);
                        if (knownExtraOps == -1)
                            WriteVarint(outSmolv, memberLen - 4);
                        else if ((uint)knownExtraOps + 4 != memberLen)
                            return null;

                        if (memberDec == 35) // Offset
                        {
                            if (memberLen != 5) return null;
                            WriteVarint(outSmolv, words[memberWordIdx + 4] - prevOffset);
                            prevOffset = words[memberWordIdx + 4];
                        }
                        else
                        {
                            for (uint i = 4; i < memberLen; ++i)
                                WriteVarint(outSmolv, words[memberWordIdx + (int)i]);
                        }

                        memberWordIdx += (int)memberLen;
                        ++count;
                    }

                    outSmolv[countLocation] = (byte)count;
                    wordIdx = memberWordIdx;
                    continue;
                }

                // Delta-encoded IDs relative to result
                int relativeCount = OpDeltaFromResult(op, knownOpsCount);
                for (int i = 0; i < relativeCount && ioffs < instrLen; ++i, ++ioffs)
                {
                    if (ioffs >= instrLen) return null;
                    uint delta = prevResult - words[wordIdx + ioffs];
                    WriteVarint(outSmolv, ZigEncode((int)delta));
                }

                if (op == SpvOp.VectorShuffleCompact)
                {
                    outSmolv.Add((byte)swizzle);
                    ioffs = (int)instrLen;
                }
                else if (OpVarRest(op, knownOpsCount))
                {
                    for (; ioffs < instrLen; ++ioffs)
                        WriteVarint(outSmolv, words[wordIdx + ioffs]);
                }
                else
                {
                    for (; ioffs < instrLen; ++ioffs)
                        Write4(outSmolv, words[wordIdx + ioffs]);
                }

                wordIdx += (int)instrLen;
            }

            // Update header size field if debug info was stripped
            if (strippedSpirvWordCount != wordCount)
            {
                byte[] headerSizeBytes = BitConverter.GetBytes((uint)strippedSpirvWordCount * 4);
                for (int i = 0; i < 4; i++)
                    outSmolv[headerSpirvSizeOffset + i] = headerSizeBytes[i];
            }

            return outSmolv.ToArray();
        }

        static string ReadStringFromWords(uint[] words, int startWord, int wordCount)
        {
            // The string is null-terminated, stored in uint32 words little-endian
            byte[] bytes = new byte[wordCount * 4];
            for (int i = 0; i < wordCount; i++)
            {
                bytes[i * 4] = (byte)(words[startWord + i] & 0xFF);
                bytes[i * 4 + 1] = (byte)((words[startWord + i] >> 8) & 0xFF);
                bytes[i * 4 + 2] = (byte)((words[startWord + i] >> 16) & 0xFF);
                bytes[i * 4 + 3] = (byte)((words[startWord + i] >> 24) & 0xFF);
            }
            int nullPos = Array.IndexOf<byte>(bytes, 0);
            if (nullPos >= 0)
                return System.Text.Encoding.UTF8.GetString(bytes, 0, nullPos);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        // --------------------------------------------------------------------------------------------
        // Public API — GetDecodedBufferSize
        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Get the required buffer size for decoding a SMOL-V program.
        /// </summary>
        /// <param name="smolv">Compressed SMOL-V data.</param>
        /// <returns>Required buffer size in bytes, or 0 if invalid.</returns>
        public static int GetDecodedBufferSize(byte[] smolv)
        {
            if (smolv == null || smolv.Length < 24)
                return 0;
            if (!CheckSmolHeader(smolv, smolv.Length))
                return 0;

            uint[] words = new uint[6];
            Buffer.BlockCopy(smolv, 0, words, 0, 24);
            return (int)words[5];
        }

        // --------------------------------------------------------------------------------------------
        // Public API — Decode
        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Decode SMOL-V back into SPIR-V.
        /// </summary>
        /// <param name="smolv">Compressed SMOL-V data.</param>
        /// <param name="flags">Optional decoding flags.</param>
        /// <returns>Decompressed SPIR-V data, or null on failure.</returns>
        public static byte[] Decode(byte[] smolv, DecodeFlags flags = DecodeFlags.None)
        {
            int bufSize = GetDecodedBufferSize(smolv);
            if (bufSize <= 0)
                return null;

            byte[] output = new byte[bufSize];
            if (DecodeInto(smolv, output, flags))
                return output;
            return null;
        }

        /// <summary>
        /// Decode SMOL-V into a pre-allocated buffer (zero-allocation path).
        /// </summary>
        public static bool DecodeInto(byte[] smolv, byte[] output, DecodeFlags flags = DecodeFlags.None)
        {
            if (smolv == null || output == null || smolv.Length < 24 || output.Length == 0)
                return false;

            int neededBufferSize = GetDecodedBufferSize(smolv);
            if (neededBufferSize <= 0)
                return false;
            if (output.Length < neededBufferSize)
                return false;

            int pos = 0;
            int end = smolv.Length;
            byte[] outSpirv = output;
            int outPos = 0;

            uint val;

            // Determine SMOL-V version
            uint[] hdrWords = new uint[6];
            Buffer.BlockCopy(smolv, 0, hdrWords, 0, 24);
            int smolVersion = (int)(hdrWords[1] >> 24);

            // Write SPIR-V header
            Write4(outSpirv, ref outPos, kSpirVHeaderMagic);
            pos += 4; // skip SMOL magic

            if (!Read4(smolv, ref pos, end, out val)) return false;
            smolVersion = (int)(val >> 24);
            val &= 0x00FFFFFFu;
            Write4(outSpirv, ref outPos, val); // version

            if (!Read4(smolv, ref pos, end, out val)) return false;
            Write4(outSpirv, ref outPos, val); // generator

            if (!Read4(smolv, ref pos, end, out val)) return false;
            Write4(outSpirv, ref outPos, val); // bound

            if (!Read4(smolv, ref pos, end, out val)) return false;
            Write4(outSpirv, ref outPos, val); // schema

            pos += 4; // skip decode buffer size

            bool beforeZeroVersion = smolVersion == 0 && (flags & DecodeFlags.Use20160831AsZeroVersion) != 0;

            int knownOpsCount = GetKnownOpsCount(smolVersion);

            uint prevResult = 0;
            uint prevDecorate = 0;

            while (pos < end)
            {
                uint instrLen;
                SpvOp op;
                if (!ReadLengthOp(smolv, ref pos, end, out instrLen, out op))
                    return false;

                bool wasSwizzle = (op == SpvOp.VectorShuffleCompact);
                if (wasSwizzle)
                    op = SpvOp.VectorShuffle;

                Write4(outSpirv, ref outPos, (instrLen << 16) | (uint)op);

                int ioffs = 1;

                // Type as varint
                if (OpHasType(op, knownOpsCount))
                {
                    if (!ReadVarint(smolv, ref pos, end, out val)) return false;
                    Write4(outSpirv, ref outPos, val);
                    ioffs++;
                }

                // Result as delta+varint
                if (OpHasResult(op, knownOpsCount))
                {
                    if (!ReadVarint(smolv, ref pos, end, out val)) return false;
                    val = prevResult + (uint)ZigDecode(val);
                    Write4(outSpirv, ref outPos, val);
                    prevResult = val;
                    ioffs++;
                }

                // Decorate: IDs relative to previous
                if (op == SpvOp.Decorate || op == SpvOp.MemberDecorate)
                {
                    if (!ReadVarint(smolv, ref pos, end, out val)) return false;
                    val = prevDecorate + (beforeZeroVersion ? val : (uint)ZigDecode(val));
                    Write4(outSpirv, ref outPos, val);
                    prevDecorate = val;
                    ioffs++;
                }

                // MemberDecorate batch decoding
                if (op == SpvOp.MemberDecorate && !beforeZeroVersion)
                {
                    if (pos >= end) return false;
                    int count = smolv[pos++];
                    int prevIndex = 0;
                    int prevOffset = 0;

                    for (int m = 0; m < count; ++m)
                    {
                        uint memberIndex;
                        if (!ReadVarint(smolv, ref pos, end, out memberIndex)) return false;
                        memberIndex += (uint)prevIndex;
                        prevIndex = (int)memberIndex;

                        uint memberDec;
                        if (!ReadVarint(smolv, ref pos, end, out memberDec)) return false;
                        int knownExtraOps = DecorationExtraOps((int)memberDec);
                        uint memberLen;
                        if (knownExtraOps == -1)
                        {
                            if (!ReadVarint(smolv, ref pos, end, out memberLen)) return false;
                            memberLen += 4;
                        }
                        else
                            memberLen = 4u + (uint)knownExtraOps;

                        if (m != 0)
                        {
                            Write4(outSpirv, ref outPos, (memberLen << 16) | (uint)op);
                            Write4(outSpirv, ref outPos, prevDecorate);
                        }
                        Write4(outSpirv, ref outPos, memberIndex);
                        Write4(outSpirv, ref outPos, memberDec);

                        if (memberDec == 35) // Offset
                        {
                            if (memberLen != 5) return false;
                            if (!ReadVarint(smolv, ref pos, end, out val)) return false;
                            val += (uint)prevOffset;
                            Write4(outSpirv, ref outPos, val);
                            prevOffset = (int)val;
                        }
                        else
                        {
                            for (uint i = 4; i < memberLen; ++i)
                            {
                                if (!ReadVarint(smolv, ref pos, end, out val)) return false;
                                Write4(outSpirv, ref outPos, val);
                            }
                        }
                    }
                    continue;
                }

                // Delta-encoded IDs relative to result
                int relativeCount = OpDeltaFromResult(op, knownOpsCount);
                bool zigDecodeVals = true;
                if (beforeZeroVersion)
                {
                    if (op != SpvOp.ControlBarrier && op != SpvOp.MemoryBarrier &&
                        op != SpvOp.LoopMerge && op != SpvOp.SelectionMerge &&
                        op != SpvOp.Branch && op != SpvOp.BranchConditional &&
                        op != SpvOp.MemoryNamedBarrier)
                        zigDecodeVals = false;
                }

                for (int i = 0; i < relativeCount && ioffs < instrLen; ++i, ++ioffs)
                {
                    if (!ReadVarint(smolv, ref pos, end, out val)) return false;
                    if (zigDecodeVals)
                        val = (uint)ZigDecode(val);
                    Write4(outSpirv, ref outPos, prevResult - val);
                }

                if (wasSwizzle && instrLen <= 9)
                {
                    uint swizzle = smolv[pos++];
                    if (instrLen > 5) Write4(outSpirv, ref outPos, (swizzle >> 6) & 3);
                    if (instrLen > 6) Write4(outSpirv, ref outPos, (swizzle >> 4) & 3);
                    if (instrLen > 7) Write4(outSpirv, ref outPos, (swizzle >> 2) & 3);
                    if (instrLen > 8) Write4(outSpirv, ref outPos, swizzle & 3);
                }
                else if (OpVarRest(op, knownOpsCount))
                {
                    for (; ioffs < instrLen; ++ioffs)
                    {
                        if (!ReadVarint(smolv, ref pos, end, out val)) return false;
                        Write4(outSpirv, ref outPos, val);
                    }
                }
                else
                {
                    for (; ioffs < instrLen; ++ioffs)
                    {
                        if (!Read4(smolv, ref pos, end, out val)) return false;
                        Write4(outSpirv, ref outPos, val);
                    }
                }
            }

            if (neededBufferSize != outPos)
                return false;

            return true;
        }
    }
}