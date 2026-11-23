using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Oathx.Entity
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public struct EncryValue
    {
        public int v;
        public int x;
        public int a;

        /// <summary>
        /// encry value constructor
        /// </summary>
        /// <param name="value"></param>
        public EncryValue(int value)
        {
            Random random = new Random();
            x = random.Next(0, VConst.XOR_RANDOM_UPPER);
            a = random.Next(-VConst.ADD_RANDOM_UPPER, VConst.ADD_RANDOM_UPPER);
            v = (value ^ x) + a;
        }

        /// <summary>
        /// set value
        /// </summary>
        /// <param name="value"></param>
        public void Set(int value)
        {
            Random random = new Random();
            x = random.Next(0, VConst.XOR_RANDOM_UPPER);
            a = random.Next(-VConst.ADD_RANDOM_UPPER, VConst.ADD_RANDOM_UPPER);
            v = (value ^ x) + a;
        }

        /// <summary>
        /// set value only
        /// </summary>
        /// <param name="value"></param>
        public void SetValueOnly(int value)
        {
            if (x == 0 && a == 0)
            {
                Random random = new Random();
                x = random.Next(0, VConst.XOR_RANDOM_UPPER);
                a = random.Next(-VConst.ADD_RANDOM_UPPER, VConst.ADD_RANDOM_UPPER);
            }

            v = (value ^ x) + a;
        }

        /// <summary>
        /// value getter    
        /// </summary>
        public int Value
        {
            get {
                return (v - a) ^ x;
            } 
        }

        /// <summary>
        /// equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is EncryValue other && this.Value == other.Value;
        }

        /// <summary>
        /// get hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(EncryValue left, EncryValue right)
        {
            return left.Value == right.Value;
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(EncryValue left, EncryValue right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// value constant
    /// </summary>
    public static class VConst
    {
        public const int XOR_RANDOM_UPPER = 1000;
        public const int ADD_RANDOM_UPPER = 1000;
    }

    /// <summary>
    /// numerical value struct
    /// </summary>
    public struct Property : IEquatable<Property>, IComparable<Property>
    {
        /// <summary>
        /// encrypted base value
        /// </summary>
        [JsonProperty]
        private EncryValue encryBaseValue;

        /// <summary>
        /// encrypted add value
        /// </summary>
        [JsonProperty]
        private EncryValue encryAddValue;

        /// <summary>
        /// encrypted multiply value
        /// </summary>
        [JsonProperty]
        private EncryValue encryMultiplyValue;

        /// <summary>
        /// base value
        /// </summary>
        [JsonIgnore]
        public int baseValue
        {
            get => encryBaseValue.Value;
            set => encryBaseValue.SetValueOnly(value);
        }

        /// <summary>
        /// add value
        /// </summary>
        [JsonIgnore]
        public int addValue
        {
            get => encryAddValue.Value;
            set => encryAddValue.SetValueOnly(value);
        }

        /// <summary>
        /// multiply value
        /// </summary>
        [JsonIgnore]
        public int mulValue
        {
            get => encryMultiplyValue.Value;
            set => encryMultiplyValue.SetValueOnly(value);
        }

        /// <summary>
        /// final value
        /// </summary>        
        [JsonIgnore]
        public int Value
        { get => (int)((baseValue + addValue) * (1 + mulValue / 100.0)); }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="baseValue"></param>
        public Property(int baseValue)
            : this(baseValue, 0, 0)
        {

        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="baseValue"></param>
        /// <param name="addValue"></param>
        /// <param name="multiplyValue"></param>
        public Property(int baseValue, int addValue, int multiplyValue)
        {
            encryBaseValue = new EncryValue(baseValue);
            encryAddValue = new EncryValue(addValue);
            encryMultiplyValue = new EncryValue(multiplyValue);
        }

        /// <summary>
        /// default zero number
        /// </summary>
        [JsonIgnore]
        public static readonly Property Zero = new Property(0, 0, 0);

        /// <summary>
        /// default one number
        /// </summary>
        [JsonIgnore]
        public static readonly Property One = new Property(1, 0, 0);

        /// <summary>
        /// is valid
        /// </summary>
        [JsonIgnore]
        public bool IsValid
        { get => baseValue >= 0 && addValue >= 0 && mulValue >= 0; }

        /// <summary>
        /// is zero
        /// </summary>
        [JsonIgnore]
        public bool IsZero
        { get => baseValue == 0 && addValue == 0 && mulValue == 0; }

        /// <summary>
        /// is positive
        /// </summary>
        [JsonIgnore]
        public bool IsPositive
        { get => Value > 0; }

        /// <summary>
        /// base value setter
        /// </summary>
        /// <param name="baseValue"></param>
        public void SetBaseValue(int baseValue)
        {
            encryBaseValue.SetValueOnly(baseValue);
        }

        /// <summary>
        /// base value getter
        /// </summary>
        /// <returns></returns>
        public int GetBaseValue()
        {
            return encryBaseValue.Value;
        }

        /// <summary>
        /// set add value
        /// </summary>
        /// <param name="addValue"></param>
        public void SetAddValue(int addValue)
        {
            encryAddValue.SetValueOnly(addValue);
        }

        /// <summary>
        /// get add value
        /// </summary>
        /// <returns></returns>
        public int GetAddValue()
        {
            return encryAddValue.Value;
        }

        /// <summary>
        /// multiply value setter
        /// </summary>
        /// <param name="multiplyValue"></param>
        public void SetMulValue(int multiplyValue)
        {
            encryMultiplyValue.SetValueOnly(multiplyValue);
        }

        /// <summary>
        /// multiply value getter
        /// </summary>
        /// <returns></returns>
        public int GetMulValue()
        {
            return encryMultiplyValue.Value;
        }

        /// <summary>
        /// set all values
        /// </summary>
        /// <param name="baseValue"></param>
        /// <param name="addValue"></param>
        /// <param name="multiplyValue"></param>
        public void SetValues(int baseValue, int addValue, int multiplyValue)
        {
            encryBaseValue.SetValueOnly(baseValue);
            encryAddValue.SetValueOnly(addValue);
            encryMultiplyValue.SetValueOnly(multiplyValue);
        }

        /// <summary>
        /// with base value
        /// </summary>
        /// <param name="baseValue"></param>
        /// <returns></returns>

        public Property WithBaseValue(int baseValue)
        {
            SetBaseValue(baseValue);
            return this;
        }

        /// <summary>
        /// with add value
        /// </summary>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public Property WithAddValue(int addValue)
        {
            SetAddValue(addValue);
            return this;
        }

        /// <summary>
        /// multiply value
        /// </summary>
        /// <param name="multiplyValue"></param>
        /// <returns></returns>
        public Property WithMultiplyValue(int multiplyValue)
        {
            SetMulValue(multiplyValue);
            return this;
        }

        /// <summary>
        /// add base value
        /// </summary>
        /// <param name="value"></param>
        public void AddBaseValue(int value)
        {
            SetBaseValue(baseValue + value);
        }

        /// <summary>
        /// add add value
        /// </summary>
        /// <param name="value"></param>
        public void AddAddValue(int value)
        {
            SetAddValue(addValue + value);
        }

        /// <summary>
        /// add multiply value
        /// </summary>
        /// <param name="value"></param>
        public void AddMulValue(int value)
        {
            SetMulValue(mulValue + value);
        }

        /// <summary>
        /// override int conversion
        /// </summary>
        /// <param name="number"></param>
        public static implicit operator int(Property number)
        {
            return number.Value;
        }

        /// <summary>
        /// override long conversion
        /// </summary>
        /// <param name="number"></param>
        public static implicit operator long(Property number)
        {
            return number.Value;
        }

        /// <summary>
        /// override Number conversion
        /// </summary>
        /// <param name="baseValue"></param>
        public static implicit operator Property(int baseValue)
        {
            return new Property(baseValue);
        }

        /// <summary>
        /// override Number conversion
        /// </summary>
        /// <param name="baseValue"></param>
        public static implicit operator Property(long baseValue)
        {
            return new Property((int)baseValue);
        }

        /// <summary>
        /// override equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Property number && Equals(number);
        }

        /// <summary>
        /// equals
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Property other)
        {
            return baseValue == other.baseValue && addValue == other.addValue && mulValue == other.mulValue;
        }

        /// <summary>
        /// get hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(baseValue, addValue, mulValue);
        }

        /// <summary>
        /// compare to
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Property other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Property left, Property right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Property left, Property right)
        {
            return !(left == right);
        }

        /// <summary>
        /// operator >
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(Property left, Property right)
        {
            return left.Value > right.Value;
        }

        /// <summary>
        /// operator <
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <(Property left, Property right)
        {
            return left.Value < right.Value;
        }

        /// <summary>
        /// operator >=
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >=(Property left, Property right)
        {
            return left.Value >= right.Value;
        }

        /// <summary>
        /// operator <=
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <=(Property left, Property right)
        {
            return left.Value <= right.Value;
        }

        /// <summary>
        /// operator +
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Property operator +(Property left, Property right)
        {
            return new Property(
                left.baseValue + right.baseValue,
                left.addValue + right.addValue,
                left.mulValue + right.mulValue
            );
        }

        /// <summary>
        /// operator -
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Property operator -(Property left, Property right)
        {
            return new Property(
                left.baseValue - right.baseValue,
                left.addValue - right.addValue,
                left.mulValue - right.mulValue
            );
        }

        /// <summary>
        /// to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"v=({baseValue} + {addValue}) * (1 + {mulValue})";
        }
    }

    /// <summary>
    /// notify changed interface
    /// </summary>
    public interface INotifyChanged
    {
        /// <summary>
        /// notify changed
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rawAttrId"></param>
        /// <param name="old"></param>
        /// <param name="attrId"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        bool OnNotifyChanged(XEntity entity, int rawAttrId, int old, int attrId, int val);
    }

    /// <summary>
    /// encry value operator type
    /// </summary>
    public enum EncryValueOperatorType
    {
        Set,
        Add,
        Mul
    }

    /// <summary>
    /// numerical attributes container
    /// </summary>
    [Serializable]
    public class XNumerical
    {
        /// <summary>
        /// all number attributes
        /// </summary>
        [JsonProperty]
        public Property[] attrs
        { get; set; } = new Property[0];

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="count"></param>
        public XNumerical()
        {
        }

        /// <summary>
        /// get all attributes
        /// </summary>
        /// <returns></returns>
        public Property[] GetAttrs()
        {
            return attrs;
        }

        /// <summary>
        /// set attribute
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public void SetAttr(int index, int newValue)
        {
            attrs[index].SetBaseValue(newValue);
        }

        /// <summary>
        /// get attribute
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetAttr(int index)
        {
            return attrs[index].Value;
        }

        /// <summary>
        /// get base value
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetBaseValue(int index)
        {
            return attrs[index].GetBaseValue();
        }

        /// <summary>
        /// set add value
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetAddValue(int index, int value)
        {
            attrs[index].SetAddValue(value);
        }

        /// <summary>
        /// get add value
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetAddValue(int index)
        {
            return attrs[index].GetAddValue();
        }

        /// <summary>
        /// get mul value
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetMulValue(int index, int value)
        {
            attrs[index].SetMulValue(value);
        }

        /// <summary>
        /// get mul value
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetMulValue(int index)
        {
            return attrs[index].GetMulValue();
        }

        /// <summary>
        /// add base value
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void AddBaseValue(int index, int value)
        {
            attrs[index].AddBaseValue(value);
        }

        /// <summary>
        /// set base value
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Set(int index, int value)
        {
            attrs[index].SetBaseValue(value);
        }

        /// <summary>
        /// add value
        /// </summary>
        /// <param name="equip"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void Add(bool equip, int index, int value)
        {
            if (!equip)
                value *= -1;

            attrs[index].AddAddValue(value);
        }

        /// <summary>
        /// mul value
        /// </summary>
        /// <param name="equip"></param>
        /// <param name="attrId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void Mul(bool equip, int attrId, int value)
        {
            if (!equip)
                value *= -1;

            attrs[attrId].AddMulValue(value);
        }

        /// <summary>
        /// apply attributes
        /// </summary>
        /// <param name="attrs"></param>
        public void ApplyAttrs(Dictionary<int, int> applys)
        {
            if (attrs.Length <= 0 && applys.Count > 0)
            {
                List<Property> properties = new List<Property>(applys.Count);
                for (int index = 0; index < applys.Count; index++)
                {
                    properties.Add(new Property(0));
                }

                attrs = properties.ToArray();
            }

            foreach (KeyValuePair<int, int> it in applys)
            {
                SetAttr(it.Key, it.Value);
            }
        }

        /// <summary>
        /// copy attributes
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public virtual bool CopyAttrs(out Property[] properties)
        {
            properties = new Property[attrs.Length];
            if (attrs.Length <= 0)
                return false;

            for (int index = 0; index < attrs.Length; index++)
            {
                properties[index] = new Property(attrs[index].baseValue, attrs[index].addValue, attrs[index].mulValue);
            }

            return true;
        }

        /// <summary>
        /// copy attributes
        /// </summary>
        /// <returns></returns>
        public Property[] CopyAttrs()
        {
            var properties = new Property[attrs.Length];
            for (int index = 0; index < attrs.Length; index++)
            {
                properties[index] = new Property(attrs[index].baseValue, attrs[index].addValue, attrs[index].mulValue);
            }

            return properties;
        }

        /// <summary>
        /// clone attributes
        /// </summary>
        /// <returns></returns>
        public virtual XNumerical Clone()
        {
            XNumerical clone = new XNumerical();

            List<Property> properties = new List<Property>(attrs.Length);
            for (int index = 0; index < attrs.Length; index++)
            {
                properties[index] = new Property(attrs[index].baseValue, 
                    attrs[index].addValue, attrs[index].mulValue);
            }

            clone.attrs = properties.ToArray(); 
            return clone;
        }

        /// <summary>
        /// this indexer
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int this[int index]
        {
            get
            {
                return attrs[index].Value;
            }

            set
            {
                attrs[index] = value;
            }
        }

        /// <summary>
        /// dump all attributes
        /// </summary>
        public void Dump()
        {
            StringBuilder sb = new StringBuilder();
            for(int index=0; index<attrs.Length; index++)
            {
                sb.AppendLine($"[{index}]{attrs[index]}");
            }

            UnityEngine.Debug.Log(sb.ToString());
        }
    }
}
