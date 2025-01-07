# ED63Trans  
  
贴吧地址： https://tieba.baidu.com/p/9387522169  
  
首先感谢Github上的ZhenjianYang、Kyuuhachi、Sewer56几位大佬的开源项目，才能让我制作出本次3RD的汉化补丁。  
  
汉化补丁文本以娱乐通版本文本为基础进行汉化，新文本和一些不好提取的文本是我手动翻译的。数据脚本使用Kyuuhachi的LB-ARK插件来进行Mod式汉化，字体文件则是通过内存注入，主程序的文本是修改程序运行时代码来达到汉化目的，所以可能会被杀毒软件报错，还请注意。 
  
游戏文本版权归娱乐通所有。  
  
本汉化补丁由Jelly制作，转载请注明作者和出处。  
　
#### 以下是汉化补丁的汉化情况：  
•	剧情：		100%  
•	道具/装备：	100%  
•	魔法：		100%  
•	书籍：		100%  
•	连锁技：		100%  
•	料理手册：	100%  
•	角色：		100%  
•	地名：		100%  
•	标题：		100%  
•	商店：		100%  
•	小游戏：		100%  
•	魔兽数据：	100%  
•	魔兽手册：	100%  
•	行为脚本：	0%，即as*._dt，角色的战斗行为脚本，我不会解析，而简单粗暴的文本替换会引发游戏崩溃。  
•	主程序：		汉化了我见到的字符串。  
•	图片：		替换的图片有地名、剧情提示两种，其他的都是界面UI和小游戏，我就不替换了。低清的我直接用的娱乐通图片，高清的是我手动PS的。  
　  
Q：兼容语音补丁和战斗语音补丁么？  
A：兼容。  
　  
Q：支持steam成就解锁么？  
A：可以解锁。  
　  
Q：可以用英文存档继续玩么？  
A：不兼容，黑屏的存档加载后会直接卡屏，通关存档可继承，其他存档可加载但不清楚会不会有BUG。  
　  
Q：可以4K游玩么？  
A：1.0.1补丁可进行4K游戏，如果有BUG在贴吧回帖。  
  
Q：可以换字体么？  
A：不推荐，如果你懂编程可以用本项目生成字库。  

Q：装了语音包没语音？  
A：语音包插件不兼容高版本游戏，进入下方链接下载新版本插件：[SoraVoice](https://github.com/ZhenjianYang/SoraVoice/releases/tag/20230823)  
  
Q：打完补丁为什么是乱码？  
A：汉化程序被杀毒软件杀了。  
  
补丁有什么问题都可以回帖，下面是一些遇到BUG的建议，可以帮助我定位BUG。  
　  
1.	如果进入战斗时崩溃闪退，那么删除d3dxof.dll文件，进入游戏记录一下所有魔兽的名字和地点。  
2.	如果换地图时崩溃闪退，那么删除data\ED6_DT30\ t_town._dt文件，这是地名文件，进入游戏记录一下地图名。  
3.	对话时出现崩溃/字符错误/乱码/英文等问题，把前后文本记录一下。  

#### 补丁使用方法：  
直接解压，替换游戏原文件即可。  
如果你使用了语音补丁，请删除游戏\ voice\ scena内的所有文件，但不要删除scena文件夹。　  
　  
## 感谢：  
脚本转换、MOD外挂: [Aureole Suite](https://github.com/Aureole-Suite)  
脚本转换: [EDDecompiler](https://github.com/ZhenjianYang/EDDecompiler)  
字库解析: [SoraTranslation-Tools](https://github.com/ZhenjianYang/SoraTranslation-Tools)  
图片转换: [KisekiCHConverter](https://github.com/Sewer56/Kiseki-Texture-Tool)  
字体：[霞鹜文楷](https://github.com/lxgw/LxgwWenKai)  
  
