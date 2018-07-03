// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: User.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Ch {

  /// <summary>Holder for reflection information generated from User.proto</summary>
  public static partial class UserReflection {

    #region Descriptor
    /// <summary>File descriptor for User.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static UserReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgpVc2VyLnByb3RvEgJjaCL7AQoEVXNlchISCgpjb3N0X2xpbWl0GAEgASgN",
            "EiUKB21hcmtldHMYAiADKAsyFC5jaC5Vc2VyLk1hcmtldEVudHJ5EicKCXRy",
            "ZWFzdXJlcxgDIAMoCzIULmNoLlVzZXIuVHJlYXN1cmVCb3gaRQoLTWFya2V0",
            "RW50cnkSDwoHY2FyZF9pZBgBIAEoDRITCgt0b2tlbl9wcmljZRgCIAEoDRIQ",
            "CghzYWxlX2VuZBgDIAEoDRpICgtUcmVhc3VyZUJveBIPCgdjYXJkX2lkGAEg",
            "ASgNEhQKDHRva2VuX2Ftb3VudBgCIAEoDRISCgpsb2NrdXBfZW5kGAMgASgN",
            "YgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Ch.User), global::Ch.User.Parser, new[]{ "CostLimit", "Markets", "Treasures" }, null, null, new pbr::GeneratedClrTypeInfo[] { new pbr::GeneratedClrTypeInfo(typeof(global::Ch.User.Types.MarketEntry), global::Ch.User.Types.MarketEntry.Parser, new[]{ "CardId", "TokenPrice", "SaleEnd" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Ch.User.Types.TreasureBox), global::Ch.User.Types.TreasureBox.Parser, new[]{ "CardId", "TokenAmount", "LockupEnd" }, null, null, null)})
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class User : pb::IMessage<User> {
    private static readonly pb::MessageParser<User> _parser = new pb::MessageParser<User>(() => new User());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<User> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ch.UserReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public User() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public User(User other) : this() {
      costLimit_ = other.costLimit_;
      markets_ = other.markets_.Clone();
      treasures_ = other.treasures_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public User Clone() {
      return new User(this);
    }

    /// <summary>Field number for the "cost_limit" field.</summary>
    public const int CostLimitFieldNumber = 1;
    private uint costLimit_;
    /// <summary>
    ///the user's allowable total cost of deck cards.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint CostLimit {
      get { return costLimit_; }
      set {
        costLimit_ = value;
      }
    }

    /// <summary>Field number for the "markets" field.</summary>
    public const int MarketsFieldNumber = 2;
    private static readonly pb::FieldCodec<global::Ch.User.Types.MarketEntry> _repeated_markets_codec
        = pb::FieldCodec.ForMessage(18, global::Ch.User.Types.MarketEntry.Parser);
    private readonly pbc::RepeatedField<global::Ch.User.Types.MarketEntry> markets_ = new pbc::RepeatedField<global::Ch.User.Types.MarketEntry>();
    /// <summary>
    ///current market entry. 
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Ch.User.Types.MarketEntry> Markets {
      get { return markets_; }
    }

    /// <summary>Field number for the "treasures" field.</summary>
    public const int TreasuresFieldNumber = 3;
    private static readonly pb::FieldCodec<global::Ch.User.Types.TreasureBox> _repeated_treasures_codec
        = pb::FieldCodec.ForMessage(26, global::Ch.User.Types.TreasureBox.Parser);
    private readonly pbc::RepeatedField<global::Ch.User.Types.TreasureBox> treasures_ = new pbc::RepeatedField<global::Ch.User.Types.TreasureBox>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Ch.User.Types.TreasureBox> Treasures {
      get { return treasures_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as User);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(User other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (CostLimit != other.CostLimit) return false;
      if(!markets_.Equals(other.markets_)) return false;
      if(!treasures_.Equals(other.treasures_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (CostLimit != 0) hash ^= CostLimit.GetHashCode();
      hash ^= markets_.GetHashCode();
      hash ^= treasures_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (CostLimit != 0) {
        output.WriteRawTag(8);
        output.WriteUInt32(CostLimit);
      }
      markets_.WriteTo(output, _repeated_markets_codec);
      treasures_.WriteTo(output, _repeated_treasures_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (CostLimit != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(CostLimit);
      }
      size += markets_.CalculateSize(_repeated_markets_codec);
      size += treasures_.CalculateSize(_repeated_treasures_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(User other) {
      if (other == null) {
        return;
      }
      if (other.CostLimit != 0) {
        CostLimit = other.CostLimit;
      }
      markets_.Add(other.markets_);
      treasures_.Add(other.treasures_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            CostLimit = input.ReadUInt32();
            break;
          }
          case 18: {
            markets_.AddEntriesFrom(input, _repeated_markets_codec);
            break;
          }
          case 26: {
            treasures_.AddEntriesFrom(input, _repeated_treasures_codec);
            break;
          }
        }
      }
    }

    #region Nested types
    /// <summary>Container for nested types declared in the User message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static partial class Types {
      public sealed partial class MarketEntry : pb::IMessage<MarketEntry> {
        private static readonly pb::MessageParser<MarketEntry> _parser = new pb::MessageParser<MarketEntry>(() => new MarketEntry());
        private pb::UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pb::MessageParser<MarketEntry> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::Ch.User.Descriptor.NestedTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public MarketEntry() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public MarketEntry(MarketEntry other) : this() {
          cardId_ = other.cardId_;
          tokenPrice_ = other.tokenPrice_;
          saleEnd_ = other.saleEnd_;
          _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public MarketEntry Clone() {
          return new MarketEntry(this);
        }

        /// <summary>Field number for the "card_id" field.</summary>
        public const int CardIdFieldNumber = 1;
        private uint cardId_;
        /// <summary>
        ///sold card id  
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public uint CardId {
          get { return cardId_; }
          set {
            cardId_ = value;
          }
        }

        /// <summary>Field number for the "token_price" field.</summary>
        public const int TokenPriceFieldNumber = 2;
        private uint tokenPrice_;
        /// <summary>
        ///card price in token
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public uint TokenPrice {
          get { return tokenPrice_; }
          set {
            tokenPrice_ = value;
          }
        }

        /// <summary>Field number for the "sale_end" field.</summary>
        public const int SaleEndFieldNumber = 3;
        private uint saleEnd_;
        /// <summary>
        ///sale end date
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public uint SaleEnd {
          get { return saleEnd_; }
          set {
            saleEnd_ = value;
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
          return Equals(other as MarketEntry);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(MarketEntry other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (CardId != other.CardId) return false;
          if (TokenPrice != other.TokenPrice) return false;
          if (SaleEnd != other.SaleEnd) return false;
          return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
          int hash = 1;
          if (CardId != 0) hash ^= CardId.GetHashCode();
          if (TokenPrice != 0) hash ^= TokenPrice.GetHashCode();
          if (SaleEnd != 0) hash ^= SaleEnd.GetHashCode();
          if (_unknownFields != null) {
            hash ^= _unknownFields.GetHashCode();
          }
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(pb::CodedOutputStream output) {
          if (CardId != 0) {
            output.WriteRawTag(8);
            output.WriteUInt32(CardId);
          }
          if (TokenPrice != 0) {
            output.WriteRawTag(16);
            output.WriteUInt32(TokenPrice);
          }
          if (SaleEnd != 0) {
            output.WriteRawTag(24);
            output.WriteUInt32(SaleEnd);
          }
          if (_unknownFields != null) {
            _unknownFields.WriteTo(output);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
          int size = 0;
          if (CardId != 0) {
            size += 1 + pb::CodedOutputStream.ComputeUInt32Size(CardId);
          }
          if (TokenPrice != 0) {
            size += 1 + pb::CodedOutputStream.ComputeUInt32Size(TokenPrice);
          }
          if (SaleEnd != 0) {
            size += 1 + pb::CodedOutputStream.ComputeUInt32Size(SaleEnd);
          }
          if (_unknownFields != null) {
            size += _unknownFields.CalculateSize();
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(MarketEntry other) {
          if (other == null) {
            return;
          }
          if (other.CardId != 0) {
            CardId = other.CardId;
          }
          if (other.TokenPrice != 0) {
            TokenPrice = other.TokenPrice;
          }
          if (other.SaleEnd != 0) {
            SaleEnd = other.SaleEnd;
          }
          _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(pb::CodedInputStream input) {
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
                break;
              case 8: {
                CardId = input.ReadUInt32();
                break;
              }
              case 16: {
                TokenPrice = input.ReadUInt32();
                break;
              }
              case 24: {
                SaleEnd = input.ReadUInt32();
                break;
              }
            }
          }
        }

      }

      public sealed partial class TreasureBox : pb::IMessage<TreasureBox> {
        private static readonly pb::MessageParser<TreasureBox> _parser = new pb::MessageParser<TreasureBox>(() => new TreasureBox());
        private pb::UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pb::MessageParser<TreasureBox> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::Ch.User.Descriptor.NestedTypes[1]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public TreasureBox() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public TreasureBox(TreasureBox other) : this() {
          cardId_ = other.cardId_;
          tokenAmount_ = other.tokenAmount_;
          lockupEnd_ = other.lockupEnd_;
          _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public TreasureBox Clone() {
          return new TreasureBox(this);
        }

        /// <summary>Field number for the "card_id" field.</summary>
        public const int CardIdFieldNumber = 1;
        private uint cardId_;
        /// <summary>
        ///card id in this box
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public uint CardId {
          get { return cardId_; }
          set {
            cardId_ = value;
          }
        }

        /// <summary>Field number for the "token_amount" field.</summary>
        public const int TokenAmountFieldNumber = 2;
        private uint tokenAmount_;
        /// <summary>
        ///token amount in this box
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public uint TokenAmount {
          get { return tokenAmount_; }
          set {
            tokenAmount_ = value;
          }
        }

        /// <summary>Field number for the "lockup_end" field.</summary>
        public const int LockupEndFieldNumber = 3;
        private uint lockupEnd_;
        /// <summary>
        ///when it can be opened?
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public uint LockupEnd {
          get { return lockupEnd_; }
          set {
            lockupEnd_ = value;
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
          return Equals(other as TreasureBox);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(TreasureBox other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (CardId != other.CardId) return false;
          if (TokenAmount != other.TokenAmount) return false;
          if (LockupEnd != other.LockupEnd) return false;
          return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
          int hash = 1;
          if (CardId != 0) hash ^= CardId.GetHashCode();
          if (TokenAmount != 0) hash ^= TokenAmount.GetHashCode();
          if (LockupEnd != 0) hash ^= LockupEnd.GetHashCode();
          if (_unknownFields != null) {
            hash ^= _unknownFields.GetHashCode();
          }
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(pb::CodedOutputStream output) {
          if (CardId != 0) {
            output.WriteRawTag(8);
            output.WriteUInt32(CardId);
          }
          if (TokenAmount != 0) {
            output.WriteRawTag(16);
            output.WriteUInt32(TokenAmount);
          }
          if (LockupEnd != 0) {
            output.WriteRawTag(24);
            output.WriteUInt32(LockupEnd);
          }
          if (_unknownFields != null) {
            _unknownFields.WriteTo(output);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
          int size = 0;
          if (CardId != 0) {
            size += 1 + pb::CodedOutputStream.ComputeUInt32Size(CardId);
          }
          if (TokenAmount != 0) {
            size += 1 + pb::CodedOutputStream.ComputeUInt32Size(TokenAmount);
          }
          if (LockupEnd != 0) {
            size += 1 + pb::CodedOutputStream.ComputeUInt32Size(LockupEnd);
          }
          if (_unknownFields != null) {
            size += _unknownFields.CalculateSize();
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(TreasureBox other) {
          if (other == null) {
            return;
          }
          if (other.CardId != 0) {
            CardId = other.CardId;
          }
          if (other.TokenAmount != 0) {
            TokenAmount = other.TokenAmount;
          }
          if (other.LockupEnd != 0) {
            LockupEnd = other.LockupEnd;
          }
          _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(pb::CodedInputStream input) {
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
                break;
              case 8: {
                CardId = input.ReadUInt32();
                break;
              }
              case 16: {
                TokenAmount = input.ReadUInt32();
                break;
              }
              case 24: {
                LockupEnd = input.ReadUInt32();
                break;
              }
            }
          }
        }

      }

    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
