using System.Collections.Generic;
using Newtonsoft.Json;

namespace Config
{
    [System.Serializable]
    public class Attr
    {
        /// <summary>
        /// 别名
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; }

        /// <summary>
        /// 数字ID
        /// </summary>
        [JsonProperty]
        public int NumID { get; private set; }

        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// 等级
        /// </summary>
        [JsonProperty]
        public int Level { get; private set; }

        /// <summary>
        /// 走路速度
        /// </summary>
        [JsonProperty]
        public int Walk { get; private set; }

        /// <summary>
        /// 跑步速度
        /// </summary>
        [JsonProperty]
        public int Speed { get; private set; }

        /// <summary>
        /// 转向速度
        /// </summary>
        [JsonProperty]
        public int AngularSpeed { get; private set; }

        /// <summary>
        /// 力量
        /// </summary>
        [JsonProperty]
        public int Strength { get; private set; }

        /// <summary>
        /// 敏捷
        /// </summary>
        [JsonProperty]
        public int Agility { get; private set; }

        /// <summary>
        /// 智力
        /// </summary>
        [JsonProperty]
        public int Wit { get; private set; }

        /// <summary>
        /// 耐力
        /// </summary>
        [JsonProperty]
        public int Stamina { get; private set; }

        /// <summary>
        /// 精神
        /// </summary>
        [JsonProperty]
        public int Spirit { get; private set; }

        /// <summary>
        /// 生命值
        /// </summary>
        [JsonProperty]
        public int Hp { get; private set; }

        /// <summary>
        /// 法力值
        /// </summary>
        [JsonProperty]
        public int Mp { get; private set; }

        /// <summary>
        /// 攻击力
        /// </summary>
        [JsonProperty]
        public int Attack { get; private set; }

        /// <summary>
        /// 防御力
        /// </summary>
        [JsonProperty]
        public int Defense { get; private set; }

        /// <summary>
        /// 5秒回血
        /// </summary>
        [JsonProperty]
        public int Hp5Recover { get; private set; }

        /// <summary>
        /// 5秒回蓝
        /// </summary>
        [JsonProperty]
        public int Mp5Recover { get; private set; }

        /// <summary>
        /// 5秒回血百分比
        /// </summary>
        [JsonProperty]
        public int Hp5Rate { get; private set; }

        /// <summary>
        /// 5秒回蓝百分比
        /// </summary>
        [JsonProperty]
        public int Mp5Rate { get; private set; }

        /// <summary>
        /// 命中率
        /// </summary>
        [JsonProperty]
        public int HitRate { get; private set; }

        /// <summary>
        /// 闪避率
        /// </summary>
        [JsonProperty]
        public int DodgeRate { get; private set; }

        /// <summary>
        /// 格挡率
        /// </summary>
        [JsonProperty]
        public int BlockRate { get; private set; }

        /// <summary>
        /// 暴击率
        /// </summary>
        [JsonProperty]
        public int CriticalRate { get; private set; }

        /// <summary>
        /// 致命一击率
        /// </summary>
        [JsonProperty]
        public int CriticalStrikeRate { get; private set; }

        /// <summary>
        /// 圣光攻击力
        /// </summary>
        [JsonProperty]
        public int Light { get; private set; }

        /// <summary>
        /// 火焰攻击力
        /// </summary>
        [JsonProperty]
        public int Fire { get; private set; }

        /// <summary>
        /// 冰霜攻击力
        /// </summary>
        [JsonProperty]
        public int Ice { get; private set; }

        /// <summary>
        /// 黑暗攻击力
        /// </summary>
        [JsonProperty]
        public int Dark { get; private set; }

        /// <summary>
        /// 圣光抗性
        /// </summary>
        [JsonProperty]
        public int LightRes { get; private set; }

        /// <summary>
        /// 火焰抗性
        /// </summary>
        [JsonProperty]
        public int FireRes { get; private set; }

        /// <summary>
        /// 冰霜抗性
        /// </summary>
        [JsonProperty]
        public int IceRes { get; private set; }

        /// <summary>
        /// 黑暗抗性
        /// </summary>
        [JsonProperty]
        public int DarkRes { get; private set; }

        /// <summary>
        /// 受伤害转魔法
        /// </summary>
        [JsonProperty]
        public int Damage2Mp { get; private set; }

        /// <summary>
        /// 攻击速度
        /// </summary>
        [JsonProperty]
        public int AttackSpeed { get; private set; }

        /// <summary>
        /// 吸血
        /// </summary>
        [JsonProperty]
        public int SuckHp { get; private set; }

        /// <summary>
        /// 吸蓝
        /// </summary>
        [JsonProperty]
        public int SuckMp { get; private set; }

        /// <summary>
        /// 反伤
        /// </summary>
        [JsonProperty]
        public int Thorns { get; private set; }

        /// <summary>
        /// 幸运
        /// </summary>
        [JsonProperty]
        public int Lucky { get; private set; }

        /// <summary>
        /// 增伤
        /// </summary>
        [JsonProperty]
        public int IncreaseDmg { get; private set; }

        /// <summary>
        /// 减伤
        /// </summary>
        [JsonProperty]
        public int MitigationDmg { get; private set; }

        /// <summary>
        /// 升级经验
        /// </summary>
        [JsonProperty]
        public int MaxExp { get; private set; }

        /// <summary>
        /// override bool operator
        /// </summary>
        public static implicit operator bool (Attr exists)
        {
            return exists != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class AttrConfig : Config<Attr>
    {
    }

    /// <summary>
    /// export excel Attr data
    /// </summary>
    public sealed partial class Excel
    {
        public const int ATTR_HERO_WARRIOR1 = 110001;
        public const int ATTR_HERO_WARRIOR2 = 110002;
        public const int ATTR_HERO_WARRIOR3 = 110003;
        public const int ATTR_HERO_WARRIOR4 = 110004;
        public const int ATTR_HERO_WARRIOR5 = 110005;
        public const int ATTR_HERO_WARRIOR6 = 110006;
        public const int ATTR_HERO_WARRIOR7 = 110007;
        public const int ATTR_HERO_WARRIOR8 = 110008;
        public const int ATTR_HERO_WARRIOR9 = 110009;
        public const int ATTR_HERO_WARRIOR10 = 110010;
        public const int ATTR_HERO_WARRIOR11 = 110011;
        public const int ATTR_HERO_WARRIOR12 = 110012;
        public const int ATTR_HERO_WARRIOR13 = 110013;
        public const int ATTR_HERO_WARRIOR14 = 110014;
        public const int ATTR_HERO_WARRIOR15 = 110015;
        public const int ATTR_HERO_WARRIOR16 = 110016;
        public const int ATTR_HERO_WARRIOR17 = 110017;
        public const int ATTR_HERO_WARRIOR18 = 110018;
        public const int ATTR_HERO_WARRIOR19 = 110019;
        public const int ATTR_HERO_WARRIOR20 = 110020;
        public const int ATTR_HERO_WARRIOR21 = 110021;
        public const int ATTR_HERO_WARRIOR22 = 110022;
        public const int ATTR_HERO_WARRIOR23 = 110023;
        public const int ATTR_HERO_WARRIOR24 = 110024;
        public const int ATTR_HERO_WARRIOR25 = 110025;
        public const int ATTR_HERO_WARRIOR26 = 110026;
        public const int ATTR_HERO_WARRIOR27 = 110027;
        public const int ATTR_HERO_WARRIOR28 = 110028;
        public const int ATTR_HERO_WARRIOR29 = 110029;
        public const int ATTR_HERO_WARRIOR30 = 110030;
        public const int ATTR_HERO_WARRIOR31 = 110031;
        public const int ATTR_HERO_WARRIOR32 = 110032;
        public const int ATTR_HERO_WARRIOR33 = 110033;
        public const int ATTR_HERO_WARRIOR34 = 110034;
        public const int ATTR_HERO_WARRIOR35 = 110035;
        public const int ATTR_HERO_WARRIOR36 = 110036;
        public const int ATTR_HERO_WARRIOR37 = 110037;
        public const int ATTR_HERO_WARRIOR38 = 110038;
        public const int ATTR_HERO_WARRIOR39 = 110039;
        public const int ATTR_HERO_WARRIOR40 = 110040;
        public const int ATTR_HERO_WARRIOR41 = 110041;
        public const int ATTR_HERO_WARRIOR42 = 110042;
        public const int ATTR_HERO_WARRIOR43 = 110043;
        public const int ATTR_HERO_WARRIOR44 = 110044;
        public const int ATTR_HERO_WARRIOR45 = 110045;
        public const int ATTR_HERO_WARRIOR46 = 110046;
        public const int ATTR_HERO_WARRIOR47 = 110047;
        public const int ATTR_HERO_WARRIOR48 = 110048;
        public const int ATTR_HERO_WARRIOR49 = 110049;
        public const int ATTR_HERO_WARRIOR50 = 110050;
        public const int ATTR_HERO_WARRIOR51 = 110051;
        public const int ATTR_HERO_WARRIOR52 = 110052;
        public const int ATTR_HERO_WARRIOR53 = 110053;
        public const int ATTR_HERO_WARRIOR54 = 110054;
        public const int ATTR_HERO_WARRIOR55 = 110055;
        public const int ATTR_HERO_WARRIOR56 = 110056;
        public const int ATTR_HERO_WARRIOR57 = 110057;
        public const int ATTR_HERO_WARRIOR58 = 110058;
        public const int ATTR_HERO_WARRIOR59 = 110059;
        public const int ATTR_HERO_WARRIOR60 = 110060;
        public const int ATTR_HERO_WARRIOR61 = 110061;
        public const int ATTR_HERO_WARRIOR62 = 110062;
        public const int ATTR_HERO_WARRIOR63 = 110063;
        public const int ATTR_HERO_WARRIOR64 = 110064;
        public const int ATTR_HERO_WARRIOR65 = 110065;
        public const int ATTR_HERO_WARRIOR66 = 110066;
        public const int ATTR_HERO_WARRIOR67 = 110067;
        public const int ATTR_HERO_WARRIOR68 = 110068;
        public const int ATTR_HERO_WARRIOR69 = 110069;
        public const int ATTR_HERO_WARRIOR70 = 110070;
        public const int ATTR_HERO_WARRIOR71 = 110071;
        public const int ATTR_HERO_WARRIOR72 = 110072;
        public const int ATTR_HERO_WARRIOR73 = 110073;
        public const int ATTR_HERO_WARRIOR74 = 110074;
        public const int ATTR_HERO_WARRIOR75 = 110075;
        public const int ATTR_HERO_WARRIOR76 = 110076;
        public const int ATTR_HERO_WARRIOR77 = 110077;
        public const int ATTR_HERO_WARRIOR78 = 110078;
        public const int ATTR_HERO_WARRIOR79 = 110079;
        public const int ATTR_HERO_WARRIOR80 = 110080;
        public const int ATTR_HERO_WARRIOR81 = 110081;
        public const int ATTR_HERO_WARRIOR82 = 110082;
        public const int ATTR_HERO_WARRIOR83 = 110083;
        public const int ATTR_HERO_WARRIOR84 = 110084;
        public const int ATTR_HERO_WARRIOR85 = 110085;
        public const int ATTR_HERO_WARRIOR86 = 110086;
        public const int ATTR_HERO_WARRIOR87 = 110087;
        public const int ATTR_HERO_WARRIOR88 = 110088;
        public const int ATTR_HERO_WARRIOR89 = 110089;
        public const int ATTR_HERO_WARRIOR90 = 110090;
        public const int ATTR_HERO_WARRIOR91 = 110091;
        public const int ATTR_HERO_WARRIOR92 = 110092;
        public const int ATTR_HERO_WARRIOR93 = 110093;
        public const int ATTR_HERO_WARRIOR94 = 110094;
        public const int ATTR_HERO_WARRIOR95 = 110095;
        public const int ATTR_HERO_WARRIOR96 = 110096;
        public const int ATTR_HERO_WARRIOR97 = 110097;
        public const int ATTR_HERO_WARRIOR98 = 110098;
        public const int ATTR_HERO_WARRIOR99 = 110099;
        public const int ATTR_HERO_WARRIOR100 = 110100;
        public const int ATTR_HERO_MAGIC1 = 120001;
        public const int ATTR_HERO_MAGIC2 = 120002;
        public const int ATTR_HERO_MAGIC3 = 120003;
        public const int ATTR_HERO_MAGIC4 = 120004;
        public const int ATTR_HERO_MAGIC5 = 120005;
        public const int ATTR_HERO_MAGIC6 = 120006;
        public const int ATTR_HERO_MAGIC7 = 120007;
        public const int ATTR_HERO_MAGIC8 = 120008;
        public const int ATTR_HERO_MAGIC9 = 120009;
        public const int ATTR_HERO_MAGIC10 = 120010;
        public const int ATTR_HERO_MAGIC11 = 120011;
        public const int ATTR_HERO_MAGIC12 = 120012;
        public const int ATTR_HERO_MAGIC13 = 120013;
        public const int ATTR_HERO_MAGIC14 = 120014;
        public const int ATTR_HERO_MAGIC15 = 120015;
        public const int ATTR_HERO_MAGIC16 = 120016;
        public const int ATTR_HERO_MAGIC17 = 120017;
        public const int ATTR_HERO_MAGIC18 = 120018;
        public const int ATTR_HERO_MAGIC19 = 120019;
        public const int ATTR_HERO_MAGIC20 = 120020;
        public const int ATTR_HERO_MAGIC21 = 120021;
        public const int ATTR_HERO_MAGIC22 = 120022;
        public const int ATTR_HERO_MAGIC23 = 120023;
        public const int ATTR_HERO_MAGIC24 = 120024;
        public const int ATTR_HERO_MAGIC25 = 120025;
        public const int ATTR_HERO_MAGIC26 = 120026;
        public const int ATTR_HERO_MAGIC27 = 120027;
        public const int ATTR_HERO_MAGIC28 = 120028;
        public const int ATTR_HERO_MAGIC29 = 120029;
        public const int ATTR_HERO_MAGIC30 = 120030;
        public const int ATTR_HERO_MAGIC31 = 120031;
        public const int ATTR_HERO_MAGIC32 = 120032;
        public const int ATTR_HERO_MAGIC33 = 120033;
        public const int ATTR_HERO_MAGIC34 = 120034;
        public const int ATTR_HERO_MAGIC35 = 120035;
        public const int ATTR_HERO_MAGIC36 = 120036;
        public const int ATTR_HERO_MAGIC37 = 120037;
        public const int ATTR_HERO_MAGIC38 = 120038;
        public const int ATTR_HERO_MAGIC39 = 120039;
        public const int ATTR_HERO_MAGIC40 = 120040;
        public const int ATTR_HERO_MAGIC41 = 120041;
        public const int ATTR_HERO_MAGIC42 = 120042;
        public const int ATTR_HERO_MAGIC43 = 120043;
        public const int ATTR_HERO_MAGIC44 = 120044;
        public const int ATTR_HERO_MAGIC45 = 120045;
        public const int ATTR_HERO_MAGIC46 = 120046;
        public const int ATTR_HERO_MAGIC47 = 120047;
        public const int ATTR_HERO_MAGIC48 = 120048;
        public const int ATTR_HERO_MAGIC49 = 120049;
        public const int ATTR_HERO_MAGIC50 = 120050;
        public const int ATTR_HERO_MAGIC51 = 120051;
        public const int ATTR_HERO_MAGIC52 = 120052;
        public const int ATTR_HERO_MAGIC53 = 120053;
        public const int ATTR_HERO_MAGIC54 = 120054;
        public const int ATTR_HERO_MAGIC55 = 120055;
        public const int ATTR_HERO_MAGIC56 = 120056;
        public const int ATTR_HERO_MAGIC57 = 120057;
        public const int ATTR_HERO_MAGIC58 = 120058;
        public const int ATTR_HERO_MAGIC59 = 120059;
        public const int ATTR_HERO_MAGIC60 = 120060;
        public const int ATTR_HERO_MAGIC61 = 120061;
        public const int ATTR_HERO_MAGIC62 = 120062;
        public const int ATTR_HERO_MAGIC63 = 120063;
        public const int ATTR_HERO_MAGIC64 = 120064;
        public const int ATTR_HERO_MAGIC65 = 120065;
        public const int ATTR_HERO_MAGIC66 = 120066;
        public const int ATTR_HERO_MAGIC67 = 120067;
        public const int ATTR_HERO_MAGIC68 = 120068;
        public const int ATTR_HERO_MAGIC69 = 120069;
        public const int ATTR_HERO_MAGIC70 = 120070;
        public const int ATTR_HERO_MAGIC71 = 120071;
        public const int ATTR_HERO_MAGIC72 = 120072;
        public const int ATTR_HERO_MAGIC73 = 120073;
        public const int ATTR_HERO_MAGIC74 = 120074;
        public const int ATTR_HERO_MAGIC75 = 120075;
        public const int ATTR_HERO_MAGIC76 = 120076;
        public const int ATTR_HERO_MAGIC77 = 120077;
        public const int ATTR_HERO_MAGIC78 = 120078;
        public const int ATTR_HERO_MAGIC79 = 120079;
        public const int ATTR_HERO_MAGIC80 = 120080;
        public const int ATTR_HERO_MAGIC81 = 120081;
        public const int ATTR_HERO_MAGIC82 = 120082;
        public const int ATTR_HERO_MAGIC83 = 120083;
        public const int ATTR_HERO_MAGIC84 = 120084;
        public const int ATTR_HERO_MAGIC85 = 120085;
        public const int ATTR_HERO_MAGIC86 = 120086;
        public const int ATTR_HERO_MAGIC87 = 120087;
        public const int ATTR_HERO_MAGIC88 = 120088;
        public const int ATTR_HERO_MAGIC89 = 120089;
        public const int ATTR_HERO_MAGIC90 = 120090;
        public const int ATTR_HERO_MAGIC91 = 120091;
        public const int ATTR_HERO_MAGIC92 = 120092;
        public const int ATTR_HERO_MAGIC93 = 120093;
        public const int ATTR_HERO_MAGIC94 = 120094;
        public const int ATTR_HERO_MAGIC95 = 120095;
        public const int ATTR_HERO_MAGIC96 = 120096;
        public const int ATTR_HERO_MAGIC97 = 120097;
        public const int ATTR_HERO_MAGIC98 = 120098;
        public const int ATTR_HERO_MAGIC99 = 120099;
        public const int ATTR_HERO_MAGIC100 = 120100;
        public const int ATTR_HERO_PASTOR1 = 130001;
        public const int ATTR_HERO_PASTOR2 = 130002;
        public const int ATTR_HERO_PASTOR3 = 130003;
        public const int ATTR_HERO_PASTOR4 = 130004;
        public const int ATTR_HERO_PASTOR5 = 130005;
        public const int ATTR_HERO_PASTOR6 = 130006;
        public const int ATTR_HERO_PASTOR7 = 130007;
        public const int ATTR_HERO_PASTOR8 = 130008;
        public const int ATTR_HERO_PASTOR9 = 130009;
        public const int ATTR_HERO_PASTOR10 = 130010;
        public const int ATTR_HERO_PASTOR11 = 130011;
        public const int ATTR_HERO_PASTOR12 = 130012;
        public const int ATTR_HERO_PASTOR13 = 130013;
        public const int ATTR_HERO_PASTOR14 = 130014;
        public const int ATTR_HERO_PASTOR15 = 130015;
        public const int ATTR_HERO_PASTOR16 = 130016;
        public const int ATTR_HERO_PASTOR17 = 130017;
        public const int ATTR_HERO_PASTOR18 = 130018;
        public const int ATTR_HERO_PASTOR19 = 130019;
        public const int ATTR_HERO_PASTOR20 = 130020;
        public const int ATTR_HERO_PASTOR21 = 130021;
        public const int ATTR_HERO_PASTOR22 = 130022;
        public const int ATTR_HERO_PASTOR23 = 130023;
        public const int ATTR_HERO_PASTOR24 = 130024;
        public const int ATTR_HERO_PASTOR25 = 130025;
        public const int ATTR_HERO_PASTOR26 = 130026;
        public const int ATTR_HERO_PASTOR27 = 130027;
        public const int ATTR_HERO_PASTOR28 = 130028;
        public const int ATTR_HERO_PASTOR29 = 130029;
        public const int ATTR_HERO_PASTOR30 = 130030;
        public const int ATTR_HERO_PASTOR31 = 130031;
        public const int ATTR_HERO_PASTOR32 = 130032;
        public const int ATTR_HERO_PASTOR33 = 130033;
        public const int ATTR_HERO_PASTOR34 = 130034;
        public const int ATTR_HERO_PASTOR35 = 130035;
        public const int ATTR_HERO_PASTOR36 = 130036;
        public const int ATTR_HERO_PASTOR37 = 130037;
        public const int ATTR_HERO_PASTOR38 = 130038;
        public const int ATTR_HERO_PASTOR39 = 130039;
        public const int ATTR_HERO_PASTOR40 = 130040;
        public const int ATTR_HERO_PASTOR41 = 130041;
        public const int ATTR_HERO_PASTOR42 = 130042;
        public const int ATTR_HERO_PASTOR43 = 130043;
        public const int ATTR_HERO_PASTOR44 = 130044;
        public const int ATTR_HERO_PASTOR45 = 130045;
        public const int ATTR_HERO_PASTOR46 = 130046;
        public const int ATTR_HERO_PASTOR47 = 130047;
        public const int ATTR_HERO_PASTOR48 = 130048;
        public const int ATTR_HERO_PASTOR49 = 130049;
        public const int ATTR_HERO_PASTOR50 = 130050;
        public const int ATTR_HERO_PASTOR51 = 130051;
        public const int ATTR_HERO_PASTOR52 = 130052;
        public const int ATTR_HERO_PASTOR53 = 130053;
        public const int ATTR_HERO_PASTOR54 = 130054;
        public const int ATTR_HERO_PASTOR55 = 130055;
        public const int ATTR_HERO_PASTOR56 = 130056;
        public const int ATTR_HERO_PASTOR57 = 130057;
        public const int ATTR_HERO_PASTOR58 = 130058;
        public const int ATTR_HERO_PASTOR59 = 130059;
        public const int ATTR_HERO_PASTOR60 = 130060;
        public const int ATTR_HERO_PASTOR61 = 130061;
        public const int ATTR_HERO_PASTOR62 = 130062;
        public const int ATTR_HERO_PASTOR63 = 130063;
        public const int ATTR_HERO_PASTOR64 = 130064;
        public const int ATTR_HERO_PASTOR65 = 130065;
        public const int ATTR_HERO_PASTOR66 = 130066;
        public const int ATTR_HERO_PASTOR67 = 130067;
        public const int ATTR_HERO_PASTOR68 = 130068;
        public const int ATTR_HERO_PASTOR69 = 130069;
        public const int ATTR_HERO_PASTOR70 = 130070;
        public const int ATTR_HERO_PASTOR71 = 130071;
        public const int ATTR_HERO_PASTOR72 = 130072;
        public const int ATTR_HERO_PASTOR73 = 130073;
        public const int ATTR_HERO_PASTOR74 = 130074;
        public const int ATTR_HERO_PASTOR75 = 130075;
        public const int ATTR_HERO_PASTOR76 = 130076;
        public const int ATTR_HERO_PASTOR77 = 130077;
        public const int ATTR_HERO_PASTOR78 = 130078;
        public const int ATTR_HERO_PASTOR79 = 130079;
        public const int ATTR_HERO_PASTOR80 = 130080;
        public const int ATTR_HERO_PASTOR81 = 130081;
        public const int ATTR_HERO_PASTOR82 = 130082;
        public const int ATTR_HERO_PASTOR83 = 130083;
        public const int ATTR_HERO_PASTOR84 = 130084;
        public const int ATTR_HERO_PASTOR85 = 130085;
        public const int ATTR_HERO_PASTOR86 = 130086;
        public const int ATTR_HERO_PASTOR87 = 130087;
        public const int ATTR_HERO_PASTOR88 = 130088;
        public const int ATTR_HERO_PASTOR89 = 130089;
        public const int ATTR_HERO_PASTOR90 = 130090;
        public const int ATTR_HERO_PASTOR91 = 130091;
        public const int ATTR_HERO_PASTOR92 = 130092;
        public const int ATTR_HERO_PASTOR93 = 130093;
        public const int ATTR_HERO_PASTOR94 = 130094;
        public const int ATTR_HERO_PASTOR95 = 130095;
        public const int ATTR_HERO_PASTOR96 = 130096;
        public const int ATTR_HERO_PASTOR97 = 130097;
        public const int ATTR_HERO_PASTOR98 = 130098;
        public const int ATTR_HERO_PASTOR99 = 130099;
        public const int ATTR_HERO_PASTOR100 = 130100;
        public const int ATTR_HERO_ROBBERS1 = 140001;
        public const int ATTR_HERO_ROBBERS2 = 140002;
        public const int ATTR_HERO_ROBBERS3 = 140003;
        public const int ATTR_HERO_ROBBERS4 = 140004;
        public const int ATTR_HERO_ROBBERS5 = 140005;
        public const int ATTR_HERO_ROBBERS6 = 140006;
        public const int ATTR_HERO_ROBBERS7 = 140007;
        public const int ATTR_HERO_ROBBERS8 = 140008;
        public const int ATTR_HERO_ROBBERS9 = 140009;
        public const int ATTR_HERO_ROBBERS10 = 140010;
        public const int ATTR_HERO_ROBBERS11 = 140011;
        public const int ATTR_HERO_ROBBERS12 = 140012;
        public const int ATTR_HERO_ROBBERS13 = 140013;
        public const int ATTR_HERO_ROBBERS14 = 140014;
        public const int ATTR_HERO_ROBBERS15 = 140015;
        public const int ATTR_HERO_ROBBERS16 = 140016;
        public const int ATTR_HERO_ROBBERS17 = 140017;
        public const int ATTR_HERO_ROBBERS18 = 140018;
        public const int ATTR_HERO_ROBBERS19 = 140019;
        public const int ATTR_HERO_ROBBERS20 = 140020;
        public const int ATTR_HERO_ROBBERS21 = 140021;
        public const int ATTR_HERO_ROBBERS22 = 140022;
        public const int ATTR_HERO_ROBBERS23 = 140023;
        public const int ATTR_HERO_ROBBERS24 = 140024;
        public const int ATTR_HERO_ROBBERS25 = 140025;
        public const int ATTR_HERO_ROBBERS26 = 140026;
        public const int ATTR_HERO_ROBBERS27 = 140027;
        public const int ATTR_HERO_ROBBERS28 = 140028;
        public const int ATTR_HERO_ROBBERS29 = 140029;
        public const int ATTR_HERO_ROBBERS30 = 140030;
        public const int ATTR_HERO_ROBBERS31 = 140031;
        public const int ATTR_HERO_ROBBERS32 = 140032;
        public const int ATTR_HERO_ROBBERS33 = 140033;
        public const int ATTR_HERO_ROBBERS34 = 140034;
        public const int ATTR_HERO_ROBBERS35 = 140035;
        public const int ATTR_HERO_ROBBERS36 = 140036;
        public const int ATTR_HERO_ROBBERS37 = 140037;
        public const int ATTR_HERO_ROBBERS38 = 140038;
        public const int ATTR_HERO_ROBBERS39 = 140039;
        public const int ATTR_HERO_ROBBERS40 = 140040;
        public const int ATTR_HERO_ROBBERS41 = 140041;
        public const int ATTR_HERO_ROBBERS42 = 140042;
        public const int ATTR_HERO_ROBBERS43 = 140043;
        public const int ATTR_HERO_ROBBERS44 = 140044;
        public const int ATTR_HERO_ROBBERS45 = 140045;
        public const int ATTR_HERO_ROBBERS46 = 140046;
        public const int ATTR_HERO_ROBBERS47 = 140047;
        public const int ATTR_HERO_ROBBERS48 = 140048;
        public const int ATTR_HERO_ROBBERS49 = 140049;
        public const int ATTR_HERO_ROBBERS50 = 140050;
        public const int ATTR_HERO_ROBBERS51 = 140051;
        public const int ATTR_HERO_ROBBERS52 = 140052;
        public const int ATTR_HERO_ROBBERS53 = 140053;
        public const int ATTR_HERO_ROBBERS54 = 140054;
        public const int ATTR_HERO_ROBBERS55 = 140055;
        public const int ATTR_HERO_ROBBERS56 = 140056;
        public const int ATTR_HERO_ROBBERS57 = 140057;
        public const int ATTR_HERO_ROBBERS58 = 140058;
        public const int ATTR_HERO_ROBBERS59 = 140059;
        public const int ATTR_HERO_ROBBERS60 = 140060;
        public const int ATTR_HERO_ROBBERS61 = 140061;
        public const int ATTR_HERO_ROBBERS62 = 140062;
        public const int ATTR_HERO_ROBBERS63 = 140063;
        public const int ATTR_HERO_ROBBERS64 = 140064;
        public const int ATTR_HERO_ROBBERS65 = 140065;
        public const int ATTR_HERO_ROBBERS66 = 140066;
        public const int ATTR_HERO_ROBBERS67 = 140067;
        public const int ATTR_HERO_ROBBERS68 = 140068;
        public const int ATTR_HERO_ROBBERS69 = 140069;
        public const int ATTR_HERO_ROBBERS70 = 140070;
        public const int ATTR_HERO_ROBBERS71 = 140071;
        public const int ATTR_HERO_ROBBERS72 = 140072;
        public const int ATTR_HERO_ROBBERS73 = 140073;
        public const int ATTR_HERO_ROBBERS74 = 140074;
        public const int ATTR_HERO_ROBBERS75 = 140075;
        public const int ATTR_HERO_ROBBERS76 = 140076;
        public const int ATTR_HERO_ROBBERS77 = 140077;
        public const int ATTR_HERO_ROBBERS78 = 140078;
        public const int ATTR_HERO_ROBBERS79 = 140079;
        public const int ATTR_HERO_ROBBERS80 = 140080;
        public const int ATTR_HERO_ROBBERS81 = 140081;
        public const int ATTR_HERO_ROBBERS82 = 140082;
        public const int ATTR_HERO_ROBBERS83 = 140083;
        public const int ATTR_HERO_ROBBERS84 = 140084;
        public const int ATTR_HERO_ROBBERS85 = 140085;
        public const int ATTR_HERO_ROBBERS86 = 140086;
        public const int ATTR_HERO_ROBBERS87 = 140087;
        public const int ATTR_HERO_ROBBERS88 = 140088;
        public const int ATTR_HERO_ROBBERS89 = 140089;
        public const int ATTR_HERO_ROBBERS90 = 140090;
        public const int ATTR_HERO_ROBBERS91 = 140091;
        public const int ATTR_HERO_ROBBERS92 = 140092;
        public const int ATTR_HERO_ROBBERS93 = 140093;
        public const int ATTR_HERO_ROBBERS94 = 140094;
        public const int ATTR_HERO_ROBBERS95 = 140095;
        public const int ATTR_HERO_ROBBERS96 = 140096;
        public const int ATTR_HERO_ROBBERS97 = 140097;
        public const int ATTR_HERO_ROBBERS98 = 140098;
        public const int ATTR_HERO_ROBBERS99 = 140099;
        public const int ATTR_HERO_ROBBERS100 = 140100;
        public const int ATTR_HERO_HUNTER1 = 150001;
        public const int ATTR_HERO_HUNTER2 = 150002;
        public const int ATTR_HERO_HUNTER3 = 150003;
        public const int ATTR_HERO_HUNTER4 = 150004;
        public const int ATTR_HERO_HUNTER5 = 150005;
        public const int ATTR_HERO_HUNTER6 = 150006;
        public const int ATTR_HERO_HUNTER7 = 150007;
        public const int ATTR_HERO_HUNTER8 = 150008;
        public const int ATTR_HERO_HUNTER9 = 150009;
        public const int ATTR_HERO_HUNTER10 = 150010;
        public const int ATTR_HERO_HUNTER11 = 150011;
        public const int ATTR_HERO_HUNTER12 = 150012;
        public const int ATTR_HERO_HUNTER13 = 150013;
        public const int ATTR_HERO_HUNTER14 = 150014;
        public const int ATTR_HERO_HUNTER15 = 150015;
        public const int ATTR_HERO_HUNTER16 = 150016;
        public const int ATTR_HERO_HUNTER17 = 150017;
        public const int ATTR_HERO_HUNTER18 = 150018;
        public const int ATTR_HERO_HUNTER19 = 150019;
        public const int ATTR_HERO_HUNTER20 = 150020;
        public const int ATTR_HERO_HUNTER21 = 150021;
        public const int ATTR_HERO_HUNTER22 = 150022;
        public const int ATTR_HERO_HUNTER23 = 150023;
        public const int ATTR_HERO_HUNTER24 = 150024;
        public const int ATTR_HERO_HUNTER25 = 150025;
        public const int ATTR_HERO_HUNTER26 = 150026;
        public const int ATTR_HERO_HUNTER27 = 150027;
        public const int ATTR_HERO_HUNTER28 = 150028;
        public const int ATTR_HERO_HUNTER29 = 150029;
        public const int ATTR_HERO_HUNTER30 = 150030;
        public const int ATTR_HERO_HUNTER31 = 150031;
        public const int ATTR_HERO_HUNTER32 = 150032;
        public const int ATTR_HERO_HUNTER33 = 150033;
        public const int ATTR_HERO_HUNTER34 = 150034;
        public const int ATTR_HERO_HUNTER35 = 150035;
        public const int ATTR_HERO_HUNTER36 = 150036;
        public const int ATTR_HERO_HUNTER37 = 150037;
        public const int ATTR_HERO_HUNTER38 = 150038;
        public const int ATTR_HERO_HUNTER39 = 150039;
        public const int ATTR_HERO_HUNTER40 = 150040;
        public const int ATTR_HERO_HUNTER41 = 150041;
        public const int ATTR_HERO_HUNTER42 = 150042;
        public const int ATTR_HERO_HUNTER43 = 150043;
        public const int ATTR_HERO_HUNTER44 = 150044;
        public const int ATTR_HERO_HUNTER45 = 150045;
        public const int ATTR_HERO_HUNTER46 = 150046;
        public const int ATTR_HERO_HUNTER47 = 150047;
        public const int ATTR_HERO_HUNTER48 = 150048;
        public const int ATTR_HERO_HUNTER49 = 150049;
        public const int ATTR_HERO_HUNTER50 = 150050;
        public const int ATTR_HERO_HUNTER51 = 150051;
        public const int ATTR_HERO_HUNTER52 = 150052;
        public const int ATTR_HERO_HUNTER53 = 150053;
        public const int ATTR_HERO_HUNTER54 = 150054;
        public const int ATTR_HERO_HUNTER55 = 150055;
        public const int ATTR_HERO_HUNTER56 = 150056;
        public const int ATTR_HERO_HUNTER57 = 150057;
        public const int ATTR_HERO_HUNTER58 = 150058;
        public const int ATTR_HERO_HUNTER59 = 150059;
        public const int ATTR_HERO_HUNTER60 = 150060;
        public const int ATTR_HERO_HUNTER61 = 150061;
        public const int ATTR_HERO_HUNTER62 = 150062;
        public const int ATTR_HERO_HUNTER63 = 150063;
        public const int ATTR_HERO_HUNTER64 = 150064;
        public const int ATTR_HERO_HUNTER65 = 150065;
        public const int ATTR_HERO_HUNTER66 = 150066;
        public const int ATTR_HERO_HUNTER67 = 150067;
        public const int ATTR_HERO_HUNTER68 = 150068;
        public const int ATTR_HERO_HUNTER69 = 150069;
        public const int ATTR_HERO_HUNTER70 = 150070;
        public const int ATTR_HERO_HUNTER71 = 150071;
        public const int ATTR_HERO_HUNTER72 = 150072;
        public const int ATTR_HERO_HUNTER73 = 150073;
        public const int ATTR_HERO_HUNTER74 = 150074;
        public const int ATTR_HERO_HUNTER75 = 150075;
        public const int ATTR_HERO_HUNTER76 = 150076;
        public const int ATTR_HERO_HUNTER77 = 150077;
        public const int ATTR_HERO_HUNTER78 = 150078;
        public const int ATTR_HERO_HUNTER79 = 150079;
        public const int ATTR_HERO_HUNTER80 = 150080;
        public const int ATTR_HERO_HUNTER81 = 150081;
        public const int ATTR_HERO_HUNTER82 = 150082;
        public const int ATTR_HERO_HUNTER83 = 150083;
        public const int ATTR_HERO_HUNTER84 = 150084;
        public const int ATTR_HERO_HUNTER85 = 150085;
        public const int ATTR_HERO_HUNTER86 = 150086;
        public const int ATTR_HERO_HUNTER87 = 150087;
        public const int ATTR_HERO_HUNTER88 = 150088;
        public const int ATTR_HERO_HUNTER89 = 150089;
        public const int ATTR_HERO_HUNTER90 = 150090;
        public const int ATTR_HERO_HUNTER91 = 150091;
        public const int ATTR_HERO_HUNTER92 = 150092;
        public const int ATTR_HERO_HUNTER93 = 150093;
        public const int ATTR_HERO_HUNTER94 = 150094;
        public const int ATTR_HERO_HUNTER95 = 150095;
        public const int ATTR_HERO_HUNTER96 = 150096;
        public const int ATTR_HERO_HUNTER97 = 150097;
        public const int ATTR_HERO_HUNTER98 = 150098;
        public const int ATTR_HERO_HUNTER99 = 150099;
        public const int ATTR_HERO_HUNTER100 = 150100;
        /// <summary>
        /// config AttrConfig table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, Attr> attrConfig;
        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = "nId"></param>
        /// <returns></returns>
        public static Attr propAttrById(int nId)
        {
            Attr data;
            if (!attrConfig.TryGetValue(nId, out data))
            {
#if UNITY_EDITOR
				var error = string.Format("Can't find config {0} id {1}", typeof(Attr).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
            }

            return data;
        }
    }
}