// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace SentisFlatBuffer
{

using global::System;
using global::System.Collections.Generic;
using global::Unity.Sentis.Google.FlatBuffers;

struct Program : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static Program GetRootAsProgram(ByteBuffer _bb) { return GetRootAsProgram(_bb, new Program()); }
  public static Program GetRootAsProgram(ByteBuffer _bb, Program obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public static bool ProgramBufferHasIdentifier(ByteBuffer _bb) { return Table.__has_identifier(_bb, "STU1"); }
  public static bool VerifyProgram(ByteBuffer _bb) {Unity.Sentis.Google.FlatBuffers.Verifier verifier = new Unity.Sentis.Google.FlatBuffers.Verifier(_bb); return verifier.VerifyBuffer("STU1", false, ProgramVerify.Verify); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public Program __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public uint Version { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetUint(o + __p.bb_pos) : (uint)0; } }
  public SentisFlatBuffer.ExecutionPlan? ExecutionPlan { get { int o = __p.__offset(6); return o != 0 ? (SentisFlatBuffer.ExecutionPlan?)(new SentisFlatBuffer.ExecutionPlan()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
  public uint SegmentsOffset { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetUint(o + __p.bb_pos) : (uint)0; } }
  public SentisFlatBuffer.DataSegment? Segments(int j) { int o = __p.__offset(10); return o != 0 ? (SentisFlatBuffer.DataSegment?)(new SentisFlatBuffer.DataSegment()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int SegmentsLength { get { int o = __p.__offset(10); return o != 0 ? __p.__vector_len(o) : 0; } }

  public static Offset<SentisFlatBuffer.Program> CreateProgram(FlatBufferBuilder builder,
      uint version = 0,
      Offset<SentisFlatBuffer.ExecutionPlan> execution_planOffset = default(Offset<SentisFlatBuffer.ExecutionPlan>),
      uint segments_offset = 0,
      VectorOffset segmentsOffset = default(VectorOffset)) {
    builder.StartTable(4);
    Program.AddSegments(builder, segmentsOffset);
    Program.AddSegmentsOffset(builder, segments_offset);
    Program.AddExecutionPlan(builder, execution_planOffset);
    Program.AddVersion(builder, version);
    return Program.EndProgram(builder);
  }

  public static void StartProgram(FlatBufferBuilder builder) { builder.StartTable(4); }
  public static void AddVersion(FlatBufferBuilder builder, uint version) { builder.AddUint(0, version, 0); }
  public static void AddExecutionPlan(FlatBufferBuilder builder, Offset<SentisFlatBuffer.ExecutionPlan> executionPlanOffset) { builder.AddOffset(1, executionPlanOffset.Value, 0); }
  public static void AddSegmentsOffset(FlatBufferBuilder builder, uint segmentsOffset) { builder.AddUint(2, segmentsOffset, 0); }
  public static void AddSegments(FlatBufferBuilder builder, VectorOffset segmentsOffset) { builder.AddOffset(3, segmentsOffset.Value, 0); }
  public static VectorOffset CreateSegmentsVector(FlatBufferBuilder builder, Offset<SentisFlatBuffer.DataSegment>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static VectorOffset CreateSegmentsVectorBlock(FlatBufferBuilder builder, Offset<SentisFlatBuffer.DataSegment>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateSegmentsVectorBlock(FlatBufferBuilder builder, ArraySegment<Offset<SentisFlatBuffer.DataSegment>> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateSegmentsVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<Offset<SentisFlatBuffer.DataSegment>>(dataPtr, sizeInBytes); return builder.EndVector(); }
  public static void StartSegmentsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<SentisFlatBuffer.Program> EndProgram(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<SentisFlatBuffer.Program>(o);
  }
  public static void FinishProgramBuffer(FlatBufferBuilder builder, Offset<SentisFlatBuffer.Program> offset) { builder.Finish(offset.Value, "STU1"); }
  public static void FinishSizePrefixedProgramBuffer(FlatBufferBuilder builder, Offset<SentisFlatBuffer.Program> offset) { builder.FinishSizePrefixed(offset.Value, "STU1"); }
}


static class ProgramVerify
{
  static public bool Verify(Unity.Sentis.Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*Version*/, 4 /*uint*/, 4, false)
      && verifier.VerifyTable(tablePos, 6 /*ExecutionPlan*/, SentisFlatBuffer.ExecutionPlanVerify.Verify, false)
      && verifier.VerifyField(tablePos, 8 /*SegmentsOffset*/, 4 /*uint*/, 4, false)
      && verifier.VerifyVectorOfTables(tablePos, 10 /*Segments*/, SentisFlatBuffer.DataSegmentVerify.Verify, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
