#region

using System.Text;
using System.Text.RegularExpressions;
using static DatTrans.TransChar;

#endregion

namespace DatTrans.Dats;

public class QuizDatItem
{
    public ushort Entry;
    public ushort Id;
    public ushort Answer;

    //1问题 4答案 1评论
    public ushort[] TextsEntries = new ushort[6];

    public string[] Texts = new string[6];
}

public class QuizDat
{
    public static List<QuizDatItem>? Parse(string file, Encoding encoding)
    {
        if (!File.Exists(file)) return null;
        var itemList = new List<QuizDatItem>();
        using var fs = new FileStream(file, FileMode.Open);
        for (var i = 0; i < 0xffff; i++)
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;
            var item = new QuizDatItem
            {
                Entry = fs.ReadUshort()
            };

            itemList.Add(item);
            var pos = fs.Position;
            fs.Seek(item.Entry, SeekOrigin.Begin);
            item.Id = fs.ReadUshort();
            item.Answer = fs.ReadUshort();
            for (int j = 0; j < item.Texts.Length; j++)
            {
                item.TextsEntries[j] = fs.ReadUshort();
            }
            for (int j = 0; j < item.Texts.Length; j++)
            {
                fs.Seek(item.TextsEntries[j], SeekOrigin.Begin);
                var ascii = fs.ReadAsciiBytes();
                item.Texts[j] = encoding.GetString(ascii);
            }
            fs.Seek(pos, SeekOrigin.Begin);
        }

        return itemList;
    }

    internal static void Generate(string file, List<QuizDatItem> xseedList, List<QuizDatItem> xseedVoiceList, List<QuizDatItem>? yltList)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        var buffer = new byte[xseedList.Count * 2]; //entrys
        fs.Write(buffer);
        foreach (var xseedItem in xseedList)
        {
            var voiceItem = xseedVoiceList.Find(x => x.Id == xseedItem.Id);
            var yltItem = yltList?.Find(x => x.Id == xseedItem.Id);

            xseedItem.Entry = (ushort)fs.Position;
            //id
            fs.WriteUshort(xseedItem.Id);
            //answer
            fs.WriteUshort(xseedItem.Answer);
            //texts
            var entriesPos = fs.Position;
            fs.Write(new byte[6 * 2]);
            for (int i = 0; i < 6; i++)
            {
                xseedItem.TextsEntries[i] = (ushort)fs.Position;
                var text = TransText(xseedItem.Id, i, xseedItem.Texts[i], voiceItem?.Texts[i], yltItem?.Texts[i]);
                text = ReplaceClmChars(text);
                var ascii = SjisEncoding.GetBytes(text);
                fs.Write(ascii);
                fs.WriteByte(0);
            }
            var endPos = fs.Position;
            fs.Seek(entriesPos, SeekOrigin.Begin);
            for (int i = 0; i < 6; i++)
            {
                fs.WriteUshort(xseedItem.TextsEntries[i]);
            }
            fs.Seek(endPos, SeekOrigin.Begin);
        }
        fs.Seek(0, SeekOrigin.Begin);
        foreach (var xseedItem in xseedList)
            fs.WriteUshort(xseedItem.Entry);
        fs.Flush();
    }

    public static string TransText(ushort id, int index, string raw, string? voice, string? ylt)
    {
        var text = "";
        if (voice != null && Regex.IsMatch(voice, "#\\d+v"))
        {
            text += Regex.Match(voice, "#\\d+v").Value;
        }
        if (ylt != null)
        {
            text += ylt;
        }
        else
        {
            //补漏狂热级
            text += TransManics(id, index);
        }
        return text;
    }

    private static string TransManics(ushort id, int index)
    {
        string[] texts = new string[6];

        switch (id)
        {
            case 0:
                texts =
               [
                    "玲的Ｓ战技『玲·歼灭』。\u0001只有开发者知道的语源是什么？",
                    "Renne, Rend and End",
                    "Renne and Renegade",
                    "Renne, Need and End",
                    "我是玲",
                    "听起来像是法文，\u0001但其实源自英文呢。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];
            case 1:
                texts =
               [
                    "在ＦＣ中片尾曲的歌名是什么？",
                    "银之意志　金之翼",
                    "Cry for me, cry for you",
                    "I swear...",
                    "星之所在",
                    "配上约修亚的口琴，\u0001是很感人的曲子呢☆\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 2:
                texts =
               [
                    "在ＳＣ中片尾曲的歌名是什么？",
                    "银之意志　金之翼",
                    "Cry for me, cry for you",
                    "I swear...",
                    "星之所在",
                    "听到这个\u0001感觉迄今为止的努力都没有白费呢☆\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 3:
                texts =
               [
                    "在ＦＣ中最终迷宫主题曲是？",
                    "Brilliant Light of the Dark Land",
                    "Dazzling Light of the Hidden Land",
                    "Hollow Light of the Sealed Land",
                    "Faint Light of the Forgotten Land",
                    "很棒，对吧？\u0001而且我知道你在那迷路过，\u0001所以我相信你十分熟悉。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 4:
                texts =
               [
                    "在ＦＣ中常规战斗主题曲是？",
                    "Sophisticated Fight",
                    "Sophisticated Sight",
                    "Sophisticated Tight",
                    "Sophisticated Night",
                    "我听说甚至有人故意去战斗，\u0001就为了听这首歌！不过你知道么？\u0001你同样可以在３ＲＤ里听到这首歌。\u0001在哪？呵呵，这是个秘密呢。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 5:
                texts =
               [
                    "在ＳＣ中常规战斗主题曲是？",
                    "Strawberry Fight",
                    "Strepitoso Fight",
                    "Strange Fight",
                    "Street Fight",
                    "Strepitoso是一个音乐术词，意思是喧嚣的。\u0001顺便说下，这是个意大利词。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 6:
                texts =
               [
                    "ＳＣ的主题曲\u0001『银之意志　金之翼』的第一句歌词是什么？",
                    "Aoi tori",
                    "Aoi toki",
                    "Aoi sora",
                    "Aoi mori",
                    "这首歌正是基于你在ＦＣ中与\u0001理查德上校和洛伦斯少尉战斗时的音乐呢。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 7:
                texts =
               [
                    "『空之轨迹』的长度单位亚矩。\u0001１亚矩等于几公尺？",
                    "１公尺",
                    "１０００公尺",
                    "１０公尺",
                    "１００公尺",
                    "作为参考，埃尔赛尤号长４２亚矩。\u0001古罗力亚斯则有着惊人的２５０亚矩。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 8:
                texts =
               [
                    "『空之轨迹』的长度单位塞尔矩。\u0001１塞尔矩等于几公尺？",
                    "１０公尺",
                    "１００公尺",
                    "１０００公尺",
                    "１０公里",
                    "基库能够以每小时１８００塞尔矩\u0001的速度直线飞行！\u0001呵呵，令人惊叹不是么？\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 9:
                texts =
               [
                    "『空之轨迹』的长度单位里矩。\u0001１里矩等于几公尺？",
                    "１公尺",
                    "０．１公尺",
                    "０．００１公尺",
                    "０．０１公尺",
                    "顺带一提，你所钓上来的鱼\u0001就是用这个单位标注的。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 10:
                texts =
               [
                    "『空之轨迹』的重量单位托令。\u0001１托令等于几公斤？",
                    "１公斤",
                    "１０公斤",
                    "１０００公斤",
                    "１００公斤",
                    "呵呵，这个单位实在太大，\u0001所以不常使用呢。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 11:
                texts =
               [
                    "这款游戏的米拉持有上限为多少？",
                    "十亿米拉",
                    "999,999,999米拉",
                    "99,999,999米拉",
                    "9,999,999米拉",
                    "呵呵，各位伙伴的口袋肯定很大，\u0001不然怎么带上这么多钱呢？\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 12:
                texts =
               [
                    "游戏载入时，画面左上方\u0001会出现载入中的动画。\u0001在ＦＣ中共有几种版本呢？",
                    "７种",
                    "８种",
                    "９种",
                    "１０种",
                    "除了队伍角色的８人外，\u0001还有猿羊总共９种版本☆\u0001顺带一提，ＳＣ也是一样喔。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 13:
                texts =
               [
                    "游戏在载入时，画面左上方\u0001会出现载入中的动画。\u0001在本作３ＲＤ中共有几种版本呢？",
                    "１４种",
                    "１５种",
                    "１６种",
                    "１７种",
                    "就是队伍角色的人数呢。\u0001顺带一提，我最喜欢的——\u0001是拔刀的上校版本吧☆\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 14:
                texts =
               [
                    "营地菜单所显示的\u0001游戏时间上限为多少？",
                    "199:59:59",
                    "299:59:59",
                    "399:59:59",
                    "499:59:59",
                    "你有玩了多久呢？\u0001我想应该已经有人提醒过你，\u0001但游戏每天玩一个小时就好喔㈱ \u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 15:
                texts =
               [
                    "轨迹系列的金钱是什么？",
                    "罗斯",
                    "票",
                    "米拉",
                    "米立",
                    "你赚够米拉了么？\u0001这个难度的奖励相当不错，\u0001但米拉可不白给！你得自己去赢得。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 16:
                texts =
               [
                    "以下哪句歌词\u0001不属于奥利维尔的爱曲\u0001《琥珀之爱》中的歌词？",
                    "若无法实现 这份空想",
                    "最初之吻 最后之吻",
                    "交错而过的疼痛",
                    "这份爱意 永远封存",
                    "我得承认，他歌喉确实不错。\u0001当然，比不上我梦幻的嗓音就是了☆\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 17:
                texts =
               [
                    "轨迹系列发生在哪块大陆上？",
                    "艾雷西亚",
                    "亚美利亚",
                    "塞姆利亚",
                    "阿弗罗卡",
                    "它与古代文明同名……\u0001还有一种可以用来制作强大武器的石头。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 18:
                texts =
                    [
                    "守护地区和平与安全的游击士\u0001『Bracer』的语源是什么？",
                    "包覆的铠甲",
                    "抵御的盾牌",
                    "支持的手甲",
                    "劈斩的利剑",
                    "包含『Brace（支持）』跟\u0001『Bracer（手甲）』的意思。\u0001也就是协会徽章上的图案呢。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 19:
                texts =
                    [
                    "以下哪个不是游戏中的属性？",
                    "ATS",
                    "DEX",
                    "MOV",
                    "CHA",
                    "CHA是魅力呢。\u0001如果真有这种属性，那莱维肯定特别高。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 20:
                texts =
                    [
                    "以下哪个不是游戏中的状态？",
                    "愤怒",
                    "混乱",
                    "尴尬",
                    "昏迷",
                    "呵呵，如果真有这种状态，\u0001那艾丝蒂尔和约修亚一起时\u0001都会一直处于这种状态中。\u0001那她完全无法战斗了呢，真可爱㈱ \u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 21:
                texts =
                    [
                    "在ＦＣ中，\u0001队伍角色的最大等级为多少？",
                    "１００",
                    "９９",
                    "４９",
                    "５０",
                    "到了尾声，\u0001经验值都只能上升个位数，\u0001之后就只能靠魄力了呢。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 22:
                texts =
                    [
                    "在ＳＣ中，\u0001队伍角色的最大等级为多少？",
                    "９０",
                    "９９",
                    "１００",
                    "１１９",
                    "顺便一提，我的等级是……\u0001机密。呵呵。你以为我会告诉你么？\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 23:
                texts =
                    [
                    "在本作３ＲＤ中，\u0001队伍角色的最大等级为多少？",
                    "１４５",
                    "１４９",
                    "１５０",
                    "１５１",
                    "呵呵。你可能一击就能\u0001击败ＦＣ中的自己吧。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 24:
                texts =
                    [
                    "噬身之蛇的执行者\u0001《怪盗绅士》布卢布兰，\u0001也就是『怪盗Ｂ』的真面目是什么人？",
                    "导力革命之父Ａ",
                    "多情的欺诈师Ｘ",
                    "悲剧性的画家Ｙ",
                    "华丽的武术家Ｚ",
                    "虽然他跟那个\u0001放荡皇子有很多相似之处，\u0001但身世跟境遇可说是完全相反呢。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];
            case 25:
                texts =
                    [
                    "战斗地图共有几个格子？",
                    "225(15x15)",
                    "289(17x17)",
                    "256(16x16)",
                    "324(18x18)",
                    "地图上要是有巨大的敌人，\u0001同尺寸的地图就让人有种狭小的感觉。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];
            case 26:
                texts =
                    [
                    "『空之轨迹』是\u0001『英雄传说』中的第几部作品呢？",
                    "第四部",
                    "第五部",
                    "第六部",
                    "第七部",
                    "要是有时间的话，\u0001你也可以试试其他的呢。\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];
            case 27:
                texts =
                    [
                    "从小说『红曜石』里面出题。\u0001担任搬运工的主角，\u0001没有使用的假名是以下何者？",
                    "菲尔",
                    "路尼",
                    "克里斯",
                    "密休特",
                    "密休特是导力工坊坊主的名字。\u0001爱因·瑟尔纳特是以真实人物为原型……\u0001那他呢？\u0002",
                    ];
                Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];
            case 28:
                texts =
                    [
                    "从小说『人偶骑士』里面出题。\u0001操纵牵线木偶《黑魔法》，\u0001同时也是主角佩德罗的师父叫什么名字？",
                    "缇雅",
                    "卡普利",
                    "哈雷克因",
                    "加斯顿",
                    "我最喜欢的角色……嗯……\u0001是反派哈雷克吧。\u0001我们反派就得互相支持不是么☆\u0002",
                    ]; Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];
            case 29:
                texts =
                    [
                    "从小说『赌博师杰克』里面出题。\u0001东方人街的赌博高手\u0001主角杰克的别名叫什么？",
                    "胜利",
                    "逆转",
                    "孤傲",
                    "无赖",
                    "《赌博师》与《小丑》——\u0001两者都要运用巧妙的手段，\u0001感觉还蛮像的呢。\u0002",
                    ]; Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];
            case 990:
                texts =
                   [
                    "----\u0002",
                    "真是遗憾，这次就到此为止了。\u0002",
                    "真是遗憾，这次就到此为止了。\u0002",
                    "看来是勉强通过了呢。\u0001呵呵，恭喜。\u0002",
                    "哦～，挺厉害的嘛。\u0001全部都能答对还真是不简单呢。\u0002",
                    "----\u0002"
                   ]; Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];

            case 999:
                texts =
                   [
                    "#90497v呵呵，那赶快开始吧。\u0001已经做好准备了吧？\u0002",
                    "呵呵呵，仔细考虑一下哦。\u0002",
                    "呵呵，饶了我吧。\u0002",
                    "嘟嘟～，错了～。\u0002",
                    "你在做什么啊。\u0002",
                    "错—\u0002"
                   ]; Console.WriteLine($"ID:{id} - {index}已处理");
                return texts[index];
        }

        throw new Exception("未定义的文本");
    }
}