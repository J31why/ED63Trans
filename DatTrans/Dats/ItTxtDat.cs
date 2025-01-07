#region

using System.Text;
using static DatTrans.TransChar;

#endregion

namespace DatTrans.Dats;

public class ItTextDatItem
{
    public ushort Entry;
    public uint Id;
    public ushort NameOffset;
    public string Name = "";
    public ushort DescOffset;
    public string Desc = "";
}

public static class ItTxtDat
{
    public static void Generate(string file, List<ItTextDatItem> xseedList, List<ItTextDatItem> yltList)
    {
        file = Path.GetFileName(file);
        using var fs = new FileStream(file, FileMode.OpenOrCreate);
        var buffer = new byte[xseedList.Count * 2]; //entries
        fs.Write(buffer);
        foreach (var xseedItem in xseedList)
        {
            //text convert
            byte[] nameBytes = [];
            byte[] descBytes = [];
            var item = xseedItem;
            var yltItem = yltList.Find(x => x.Id == item.Id);
            if (yltItem == null)
            {
                switch (xseedItem.Id)
                {
                    //entry: 37634    id: 750         text: Time Gem          text2: STR-20%, DEF-20%, ATS-15% (#93ICast 3)
                    case 750:
                        var replacedText = ReplaceClmChars("刻耀珠");
                        nameBytes = SjisEncoding.GetBytes(replacedText);
                        replacedText = ReplaceClmChars("STR-20%, DEF-20%, ATS-15% (#93I驱动３)");
                        descBytes = SjisEncoding.GetBytes(replacedText);
                        Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                        break;
                }
            }
            else
            {
                if (xseedItem.Name is "-" or "--" or "－－－－－－－－－－" or " " or "\u25a0Unused"
                    || string.IsNullOrWhiteSpace(xseedItem.Name)) //空或无需替换的道具文本
                {
                    nameBytes = SjisEncoding.GetBytes(xseedItem.Name);
                    descBytes = SjisEncoding.GetBytes(xseedItem.Desc);
                }
                else
                {
                    if (yltItem.Name is "--" or " " or "-")
                        //xseed版新物品 id: 527,658,659
                        Console.WriteLine("空ID:" + yltItem.Id);
                    switch (xseedItem.Id)
                    {
                        //bug fix
                        case 527:
                            //Monster Guide
                            yltItem.Name = "魔兽手册";
                            yltItem.Desc = "记录收集到的魔兽情报的手册。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 658:
                            //Mirror
                            yltItem.Name = "镜子";
                            yltItem.Desc = "50%机率反射物理伤害。最大HP-20%";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 659:
                            //Reflect
                            yltItem.Name = "反射";
                            yltItem.Desc = "50%机率反射魔法伤害。最大EP-20%";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 709:
                            //木耀珠 娱乐通汉化BUG
                            yltItem.Desc = yltItem.Desc.Replace("妨害", "妨碍");
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 1210:
                            yltItem.Name = "花剑+";
                            yltItem.Desc = "【STR+100】\\n\u3000专为突刺而设计的细剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 1211:
                            yltItem.Name = "尖剑+";
                            yltItem.Desc = "【STR+200】\\n\u3000高级者用的细剑，以柔制刚的武器。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 1420:
                            //Fencer+1
                            yltItem.Name = "击剑+1";
                            yltItem.Desc = "【STR+100】\\n\u3000一把对学习基础剑术至关重要的训练用剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;

                        case 1421:
                            yltItem.Name = "击剑+2";
                            yltItem.Desc = "【STR+200】\\n\u3000一把对学习基础剑术至关重要的训练用剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        // parameter error
                        case 425:
                            yltItem.Desc = "【HP18000回复 DEF+20%】\\n\u3000榨取爪子、豆酱、肉汁精华的豪华型螃蟹料理。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1365:
                            yltItem.Desc = "【STR+500 RNG+6 ATS+25 DEX+15】\\n\u3000可以隐藏于衣服内侧的暗器型弩。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1367:
                            yltItem.Desc = "【STR+700 RNG+6 ATS+50 DEX+20】\\n\u3000曾一举改写战争格局，中世纪末期制造的巨型弓。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1370:
                            yltItem.Desc = "【STR+1000 RNG+7 ATS+75 DEX+25】\\n\u3000以法典国传统工艺加工七耀石制成的弓。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1371:
                            yltItem.Desc = "【STR+1100 RNG+7 ATS+100 DEX+30】\\n\u3000古代文明遗留的自动弓，其驱动音如同脉搏一般。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1372:
                            yltItem.Desc = "【STR+1200 RNG+8 ATS+125 DEX+35】\\n\u3000七耀教会珍藏的神弓，对要害部位百发百中。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1374:
                            yltItem.Desc = "【STR+850 RNG+6 ATS+30 ADF+30 DEX+10】\\n\u3000旅行者遗失的物品，山民使用的弓。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1375:
                            yltItem.Desc = "【STR+950 RNG+7 ATS+40 ADF+40 DEX+20】\\n\u3000旅行者遗失的物品，山民使用的弓。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1376:
                            yltItem.Desc = "【STR+1300 RNG+8 ATS+50 ADF+50 DEX+30】\\n\u3000旅行者遗失的物品，山民使用的弓。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1377:
                            yltItem.Desc = "【STR+1500 RNG+8 ATS+60 ADF+60 DEX+35】\\n\u3000以谜之矿石精炼而成，箭矢取之不尽。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1455:
                            yltItem.Desc = "【STR+500 RNG+3 ATS+50】\\n\u3000敝旧的白银法剑，被使用过的痕迹相当明显。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1459:
                            yltItem.Desc = "【STR+900 RNG+3 ATS+150】\\n\u3000法典国直属的骑士才有资格使用的法剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1461:
                            yltItem.Desc = "【STR+1100 RNG+2 ATS+200】\\n\u3000从古文明时代的坟墓中出土的法剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1462:
                            yltItem.Desc = "【STR+1200 RNG+3 ATS+250】\\n\u3000女神的法剑。为世人指明道路、驱除迷惑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1464:
                            yltItem.Desc = "【STR+1050 RNG+3 DEF/ATS/ADF/DEX+5 AGL/SPD+2】\\n\u3000旅行者遗失的物品，据说此法剑曾指向神明。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1465:
                            yltItem.Desc = "【STR+1300 RNG+3 DEF/ATS/ADF/DEX+10 AGL/SPD+5】\\n\u3000旅行者遗失的物品，据说此法剑曾指向神明。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1467:
                            yltItem.Desc =
                                "【STR+1500 RNG+3 DEF/ATS/ADF/DEX+15 AGL/SPD+10】\\n\u3000以谜之矿石精炼而成，剑身闪烁着白色光球的法 剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1006:
                            yltItem.Desc = "【STR+1000 RNG+3 DEX+10 AGL+10 SPD+5】\\n\u3000棒尖飘扬着作为力量象征的旗子，攻击范围较远。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1008:
                            yltItem.Desc = "【STR+1200 RNG+3 DEX+10 AGL+10】\\n\u3000对目标对象实施重击，能够扰乱现实的幻想之棍。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1010:
                            yltItem.Desc = "【STR+950 RNG+3 DEX+5 AGL+15 SPD+5】\\n\u3000旅行者遗失的物品，挥动的树枝轨迹上有红色花瓣飘舞。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1011:
                            yltItem.Desc = "【STR+1300 RNG+3 DEX+15 AGL+25 SPD+5】\\n\u3000旅行者遗失的物品，挥动的树枝轨迹上有红色花瓣飘舞。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1012:
                            yltItem.Desc = "【STR+1500 RNG+3 DEX+25 AGL+25 SPD+5】\\n\u3000以谜之矿石精制而成，棍体散发着温暖的光芒。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1048:
                            yltItem.Desc = "【STR+400 DEX+5 AGL+5】\\n\u3000可以融入影子中的漆黑小刀，秘密行动使用的暗器。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1051:
                            yltItem.Desc = "【STR+700 DEX+10 AGL+10】\\n\u3000让敌人也无法察觉自己被斩开的锐利双剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1055:
                            yltItem.Desc = "【STR+1100 DEX+20 AGL+20】\\n\u3000将古代剑熔化，利用其碎片翻新制作的双剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1056:
                            yltItem.Desc = "【STR+1200 DEX+25 AGL+25】\\n\u3000在传说中登场的宝剑，被讴歌为战场上的翱翔之鸟。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1058:
                            yltItem.Desc = "【STR+950 ADF+25 DEX+25】\\n\u3000旅行者遗失的物品，据说是从天而降的东西。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1059:
                            yltItem.Desc = "【STR+1300 ADF+75 DEX+25】\\n\u3000旅行者遗失的物品，据说是从天而降的东西。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1060:
                            yltItem.Desc = "【STR+1500 DEX+30 AGL+25】\\n\u3000以谜之矿石精制而成，刻有曲线花纹的双剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1094:
                            yltItem.Desc = "【STR+800 RNG+3 DEX+5】\\n\u3000金线制成的鞭子，可以将被缠绕的部位切断。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1097:
                            yltItem.Desc = "【STR+1100 RNG+3 ATS+30 ADF+30 DEX+5】\\n\u3000以古代技术制成的细长铁线，鞭身闪耀着银色的余辉。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1098:
                            yltItem.Desc = "【STR+1200 RNG+4 ATS+35 ADF+35 DEX+10】\\n\u3000用粘在一起的辉石鳞片制成的多尾鞭。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1100:
                            yltItem.Desc = "【STR+950 RNG+4 ATS+50 ADF+50 DEX+10】\\n\u3000旅行者遗失的物品，牧童常用的鞭子。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1101:
                            yltItem.Desc = "【STR+1300 RNG+4 ATS+100 ADF+100 DEX+20】\\n\u3000旅行者遗失的物品，牧童常用的鞭子。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1103:
                            yltItem.Desc = "【STR+1500 RNG+4 ATS+150 ADF+150 DEX+20】\\n\u3000以谜之矿石精制而成的七色鞭子。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1138:
                            yltItem.Desc = "【STR+400 RNG+6 DEX+5】\\n\u3000共和国乌尔努社研发的大功率导力枪的后续版本。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1141:
                            yltItem.Desc = "【STR+700 RNG+6 DEX+10】\\n\u3000共和国乌尔努社制的军用导力枪，枪身佩有刺刀。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1144:
                            yltItem.Desc = "【STR+1000 RNG+6 DEX+15】\\n\u3000以东方秘诀精炼而成，散发着香气的木制导力枪。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1145:
                            yltItem.Desc = "【STR+1100 RNG+5 DEX+20】\\n\u3000古代文明制造的导力枪，可以进行多种换装。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1146:
                            yltItem.Desc = "【STR+1200 RNG+6 DEX+25】\\n\u3000以七耀石作为子弹的传说中的古代枪，典礼用的左轮手枪。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1148:
                            yltItem.Desc = "【STR+750 RNG+6 DEX+30 SPD+5】\\n\u3000旅行者遗失的物品，可以打出在体内反射的子弹。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1149:
                            yltItem.Desc = "【STR+950 RNG+6 DEX+35 SPD+5】\\n\u3000旅行者遗失的物品，可以打出在体内反射的子弹。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1150:
                            yltItem.Desc = "【STR+1300 RNG+6 DEX+40 SPD+5】\\n\u3000旅行者遗失的物品，可以打出在体内反射的子弹。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1151:
                            yltItem.Desc = "【STR+1500 RNG+6 DEX+50】\\n\u3000以谜之矿石精炼而成的导力枪，子弹可以自动追踪敌人。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1184:
                            yltItem.Desc = "【STR+400 ATS+25 AGL+5】\\n\u3000弯曲的剑身，以敏锐与锋利著称的弯剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1187:
                            yltItem.Desc = "【STR+700 ATS+30 AGL+10】\\n\u3000亚尔特利亚法典国生产的剑，护手被设计成星杯的模样。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1190:
                            yltItem.Desc = "【STR+1000 ATS+35 AGL+15】\\n\u3000刻有祈求胜利的祷词，利贝尔王室宝剑之一。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1191:
                            yltItem.Desc = "【STR+1100 ATS+40 AGL+20】\\n\u3000用古代大树的材料制成，可以自动修复剑身崩刃的木剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1192:
                            yltItem.Desc = "【STR+1200 ATS+45 AGL+25】\\n\u3000梦幻之剑。透明的刀身依天色变化颜色也会不同。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1194:
                            yltItem.Desc = "【STR+850 ATS+20 DEX+5 AGL+5】\\n\u3000旅行者遗失的物品，据说可以斩杀世界上的任何魔物。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1195:
                            yltItem.Desc = "【STR+950 ATS+40 DEX+5 AGL+15】\\n\u3000旅行者遗失的物品，据说可以斩杀世界上的任何魔物。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1196:
                            yltItem.Desc = "【STR+1300 ATS+60 DEX+5 AGL+20】\\n\u3000旅行者遗失的物品，据说可以斩杀世界上的任何魔物。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1197:
                            yltItem.Desc = "【STR+1550 ATS+80 DEX+10 AGL+25】\\n\u3000以谜之矿石精炼而成的宝剑，永远不会断折。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1230:
                            yltItem.Desc = "【STR+650 DEX-10】\\n\u3000刀身细长的太刀，战斗时候有种几乎被曳断的感觉。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1233:
                            yltItem.Desc = "【STR+950 DEX-15】\\n\u3000于巨大刀身上加工有透明刀刃，用来迷惑敌人的双手剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1236:
                            yltItem.Desc = "【STR+1250 DEX-20】\\n\u3000一把不祥的红黑色大剑，刻有龙的纹章。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1238:
                            yltItem.Desc = "【STR+900 DEX+5 AGL+10】\\n\u3000旅行者遗失的物品，已经折断的一把巨大圣剑剑柄。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1239:
                            yltItem.Desc = "【STR+1100 DEX+10 AGL+15】\\n\u3000旅行者遗失的物品，已经折断的一把巨大圣剑剑柄。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1240:
                            yltItem.Desc = "【STR+1350 DEX+15 AGL+20】\\n\u3000旅行者遗失的物品，已经折断的一把巨大圣剑剑柄。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1241:
                            yltItem.Desc = "【STR+1550 DEX+20 AGL+25】\\n\u3000将谜之矿石精炼技术发挥至极点，锻造出的歪斜刀身大剑。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1284:
                            yltItem.Desc = "【STR+850 RNG+5 ADF+5 DEX+20 SPD+5 范围：大圆】\\n\u3000旅行者遗失的物品，可以应付一切情况的导力炮。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1285:
                            yltItem.Desc = "【STR+950 RNG+5 ADF+10 DEX+30 SPD+5 范围：大圆】\\n\u3000旅行者遗失的物品，可以应付一切情况的导力炮 。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1286:
                            yltItem.Desc = "【STR+1300 RNG+5 ADF+15 DEX+40 SPD+5 范围：大圆】\\n\u3000旅行者遗失的物品，可以应付一切情况的导力炮。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1287:
                            yltItem.Desc =
                                "【STR+1500 RNG+5 DEF/ATS/SPD+10 ADF+20 DEX+50 范围：大圆】\\n\u3000以谜之矿石精炼而成，能将空间熔化 的导力炮。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1319:
                            yltItem.Desc = "【STR+750 DEX+5 SPD+5】\\n\u3000标示气流的刻印可以充分发挥出装备者力量的武术手套。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1322:
                            yltItem.Desc = "【STR+1050 DEX+10 SPD+5】\\n\u3000由东方传来的金制护腕，装饰度与硬度得到了很好的平衡。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1323:
                            yltItem.Desc = "【STR+1150 DEX+15 SPD+10】\\n\u3000不会给敌人留下外伤，会直接对敌人造成内伤的古代护腕。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1324:
                            yltItem.Desc = "【STR+1250 DEX+20 SPD+15】\\n\u3000用被共和国视为神圣的灵峰之土制成的护腕。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1326:
                            yltItem.Desc = "【STR+1000 DEX+30 SPD+10】\\n\u3000旅行者遗失的物品，失落的女神像双臂。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1327:
                            yltItem.Desc = "【STR+1350 DEX+35 SPD+20】\\n\u3000旅行者遗失的物品，失落的女神像双臂。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1328:
                            yltItem.Desc = "【STR+1550 DEX+40 SPD+25】\\n\u3000以谜之矿石精炼而成，绽放着四色光芒的护腕。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1415:
                            yltItem.Desc = "【STR+1000 DEF/ATS/ADF/DEX/AGL+10 SPD+5】\\n\u3000旅行者遗失的物品，刀身寄宿着闪烁微光的萤火虫。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1416:
                            yltItem.Desc = "【STR+1300 DEF/ATS/ADF/DEX/AGL+15 SPD+10】\\n\u3000旅行者遗失的物品，刀身寄宿着闪烁微光的萤火虫。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1417:
                            yltItem.Desc =
                                "【STR+1550 DEF/ATS/ADF/DEX/AGL+20 SPD+15】\\n\u3000以谜之矿石精炼而成，挥动时只能看到闪亮的刀身残像 。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1424:
                            yltItem.Desc = "【STR+1000 ATS/ADF+50 DEX+25 AGL/SPD+5 亚妮拉丝专用】\\n\u3000伴随剑圣修行磨练的原训练用古刀。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1425:
                            yltItem.Name = "利剑『迅羽』";
                            yltItem.Desc = "【STR+1600 ATS/ADF+150 DEX+100 AGL/SPD+25 亚妮拉丝专用】\\n\u3000伴随剑圣修行磨练的原训练用古刀。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1503:
                            yltItem.Desc = "【STR+1200 DEX+50 AGL+20 SPD+5】\\n\u3000旅行者遗失的物品，只要刀身沾上血迹就会发出清澈声音。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1504:
                            yltItem.Desc = "【STR+1450 DEX+100 AGL+25 SPD+10】\\n\u3000以谜之矿石精炼而成，刀身闪耀着黑色的光泽。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1610:
                            yltItem.Desc = "【DEF+225 STR+20 ADF+20 SPD+5 男性专用】\\n\u3000七耀石制的全身铠甲。贯注入的光芒反射出煌煌的辉泽。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1611:
                            yltItem.Desc = "【DEF+525 STR+40 ADF+40 SPD+5 男性专用】\\n\u3000七耀石的全身铠甲。贯注入的光芒反射出煌煌的辉泽。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1612:
                            yltItem.Desc = "【DEF+825 STR+60 ADF+60 SPD+5 男性专用】\\n\u3000七耀石的全身铠甲。贯注入的光芒反射出煌煌的辉泽。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1613:
                            yltItem.Desc = "【DEF+1155 STR+80 ADF+80 SPD+5 男性专用】\\n\u3000七耀石的全身铠甲。贯注入的光芒反射出煌煌的辉泽。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1630:
                            yltItem.Desc = "【DEF+225 ATS+50 AGL+15 女性专用】\\n\u3000七耀石的防护服，溢出的光芒折射般的跳动着。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1631:
                            yltItem.Desc = "【DEF+525 ATS+100 AGL+30 女性专用】\\n\u3000七耀石的防护服，溢出的光芒折射般的跳动着。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1632:
                            yltItem.Desc = "【DEF+825 ATS+150 AGL+45 女性专用】\\n\u3000七耀石的防护服，溢出的光芒折射般的跳动着。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 1633:
                            yltItem.Desc = "【DEF+1155 ATS+200 AGL+60 女性专用】\\n\u3000七耀石的防护服，溢出的光芒折射般的跳动着。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 173:
                            yltItem.Desc =
                                "【DEF+165 ATS-35 ADF-35 AGL+35 SPD+20 MOV+5 男性专用】\\n\u3000古代的机械靴，无论走到何处都能削弱逆 境带来的影响。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 190:
                            yltItem.Desc = "【DEF+75 ATS+10 ADF+10 AGL-5 MOV+3 女性专用】\\n\u3000中世纪的长靴，可以永远获得奥秘的神力。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 191:
                            yltItem.Desc = "【DEF+105 ATS+25 ADF+25 AGL-10 MOV+4 女性专用】\\n\u3000中世纪的长靴，可以永远获得奥秘的神力。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 192:
                            yltItem.Desc = "【DEF+140 ATS+50 ADF+50 AGL-15 MOV+5 女性专用】\\n\u3000中世纪的长靴，可以永远获得奥秘的神力。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                        case 193:
                            yltItem.Desc = "【DEF+165 ATS+75 ADF+75 AGL-20 MOV+6 女性专用】\\n\u3000中世纪的长靴，可以永远获得奥秘的神力。";
                            Console.WriteLine($"ID: {xseedItem.Id} 已处理");
                            break;
                    }

                    var replacedText = ReplaceClmChars(yltItem.Name);
                    nameBytes = SjisEncoding.GetBytes(replacedText);

                    replacedText = ReplaceClmChars(yltItem.Desc);
                    descBytes = SjisEncoding.GetBytes(replacedText);
                }
            }

            xseedItem.Entry = (ushort)fs.Position;
            //id
            fs.WriteUint(xseedItem.Id);
            //name pos
            fs.WriteUshort((ushort)(fs.Position + 4));
            //desc pos
            fs.WriteUshort((ushort)(fs.Position + 2 + nameBytes.Length + 1));
            fs.Write(nameBytes);
            fs.WriteByte(0);
            fs.Write(descBytes);
            fs.WriteByte(0);
        }

        fs.Seek(0, SeekOrigin.Begin);
        foreach (var xseedItem in xseedList) fs.WriteUshort(xseedItem.Entry);
        fs.Flush();
    }

    public static List<ItTextDatItem> Parse(string file, Encoding encoding)
    {
        var itemList = new List<ItTextDatItem>();
        using var fs = new FileStream(file, FileMode.Open);
        for (var i = 0; i < 0xffff; i++)
        {
            if (itemList.Count > 0)
                if (fs.Position == itemList[0].Entry)
                    break;
            var item = new ItTextDatItem
            {
                Entry = fs.ReadUshort()
            };

            itemList.Add(item);
            var pos = fs.Position;
            fs.Seek(item.Entry, SeekOrigin.Begin);

            item.Id = fs.ReadUint();
            item.NameOffset = fs.ReadUshort();
            item.DescOffset = fs.ReadUshort();
            fs.Seek(item.NameOffset, SeekOrigin.Begin);
            var ascii = fs.ReadAsciiBytes();
            item.Name = encoding.GetString(ascii);
            fs.Seek(item.DescOffset, SeekOrigin.Begin);
            ascii = fs.ReadAsciiBytes();
            item.Desc = encoding.GetString(ascii);

            fs.Seek(pos, SeekOrigin.Begin);
        }

        return itemList;
    }
}