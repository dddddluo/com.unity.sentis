// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>
#define ENABLE_SPAN_T
#define UNSAFE_BYTEBUFFER
#define BYTEBUFFER_NO_BOUNDS_CHECK

namespace SentisFlatBuffer
{

using global::System;
using global::System.Collections.Generic;
using global::Unity.Sentis.Google.FlatBuffers;

struct BackendPartitioning : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static BackendPartitioning GetRootAsBackendPartitioning(ByteBuffer _bb) { return GetRootAsBackendPartitioning(_bb, new BackendPartitioning()); }
  public static BackendPartitioning GetRootAsBackendPartitioning(ByteBuffer _bb, BackendPartitioning obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public BackendPartitioning __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int Chains(int j) { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(__p.__vector(o) + j * 4) : (int)0; }
  public int ChainsLength { get { int o = __p.__offset(4); return o != 0 ? __p.__vector_len(o) : 0; } }
#if ENABLE_SPAN_T
  public Span<int> GetChainsBytes() { return __p.__vector_as_span<int>(4, 4); }
#else
  public ArraySegment<byte>? GetChainsBytes() { return __p.__vector_as_arraysegment(4); }
#endif
  public int[] GetChainsArray() { return __p.__vector_as_array<int>(4); }
  public SentisFlatBuffer.BackendType Backend { get { int o = __p.__offset(6); return o != 0 ? (SentisFlatBuffer.BackendType)__p.bb.GetSbyte(o + __p.bb_pos) : SentisFlatBuffer.BackendType.CPU; } }

  public static Offset<SentisFlatBuffer.BackendPartitioning> CreateBackendPartitioning(FlatBufferBuilder builder,
      VectorOffset chainsOffset = default(VectorOffset),
      SentisFlatBuffer.BackendType backend = SentisFlatBuffer.BackendType.CPU) {
    builder.StartTable(2);
    BackendPartitioning.AddChains(builder, chainsOffset);
    BackendPartitioning.AddBackend(builder, backend);
    return BackendPartitioning.EndBackendPartitioning(builder);
  }

  public static void StartBackendPartitioning(FlatBufferBuilder builder) { builder.StartTable(2); }
  public static void AddChains(FlatBufferBuilder builder, VectorOffset chainsOffset) { builder.AddOffset(0, chainsOffset.Value, 0); }
  public static VectorOffset CreateChainsVector(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddInt(data[i]); return builder.EndVector(); }
  public static VectorOffset CreateChainsVectorBlock(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateChainsVectorBlock(FlatBufferBuilder builder, ArraySegment<int> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateChainsVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<int>(dataPtr, sizeInBytes); return builder.EndVector(); }
  public static void StartChainsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddBackend(FlatBufferBuilder builder, SentisFlatBuffer.BackendType backend) { builder.AddSbyte(1, (sbyte)backend, 0); }
  public static Offset<SentisFlatBuffer.BackendPartitioning> EndBackendPartitioning(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<SentisFlatBuffer.BackendPartitioning>(o);
  }
}


static class BackendPartitioningVerify
{
  static public bool Verify(Unity.Sentis.Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyVectorOfData(tablePos, 4 /*Chains*/, 4 /*int*/, false)
      && verifier.VerifyField(tablePos, 6 /*Backend*/, 1 /*SentisFlatBuffer.BackendType*/, 1, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}
