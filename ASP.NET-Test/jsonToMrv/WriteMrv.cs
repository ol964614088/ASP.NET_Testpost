using chemaxon.formats;
using chemaxon.struc;
using chemaxon.struc.graphics;
using java.awt;
using java.util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsonToMrv
{
    public class WriteMrv
    {
        /// <summary>
        /// 间距X
        /// </summary>
        private const double SpanceX = 8;

        /// <summary>
        /// 间距Y
        /// </summary>
        private const double SpanceY = 7;

        /// <summary>
        /// 单行分支分子数量
        /// </summary>
        private const int BranchOneLineNum = 6;

        /// <summary>
        /// 分支垂直间距倍数
        /// </summary>
        private const int BranchSPFWithY = 5;

        /// <summary>
        /// 分支水平间距倍数
        /// </summary>
        private const int BranchSPFWithX = 2;

        /// <summary>
        /// 保存主线分支中分叉点所对应的分子
        /// </summary>
        private Dictionary<int, string> DicBranchSmiles;

        /// <summary>
        /// 全路线的水平路线占比
        /// </summary>
        private const double SpaceLineScale = 0.4;

        /// <summary>
        /// 正下方分子层次与分子的间距
        /// </summary>
        private const double TxtspaceWithMolInY = 1.5;

        /// <summary>
        /// 反应步次与水平线的间距
        /// </summary>
        private const double ReactionIndexSpaceWithLine = 1.5;

        /// <summary>
        /// 记录每一个水平分支需要写层次
        /// </summary>
        private Dictionary<int, List<string>> _DicListTextPosition;

        /// <summary>
        /// 分支中其他的mol距离箭头的距离(垂直)
        /// </summary>
        private const double BranchAnotherMolDistanceY = 2;

        /// <summary>
        /// 分支中其他的mol距离单侧的距离（水平）
        /// </summary>
        private const double BranchAnotherMolDistanceX = 3.5;

        /// <summary>
        /// 用于计算当前反应步数的变量
        /// </summary>
        private int ReactionSteps = 1;

        /// <summary>
        /// 分支导出用于存放mol层次位置
        /// </summary>
        private List<string> _ListTextPosition;


        #region 整个路线列表相关代码
        /// <summary>
        /// 返回整个路线列表
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public string WriteToMrvWithAllRouteWithStr(string result)
        {
            //取根路线
            List<TreeNodes> treeNodes = JsonConvert.DeserializeObject<List<TreeNodes>>(result);
            TreeNodes rootTree = new TreeNodes();
            foreach (var item in treeNodes)
            {
                if (item.PID == 0)
                {
                    rootTree = item;
                }
            }
            //树绘图
            MDocument md = WriteTree(rootTree);

            return MolExporter.exportToFormat(md, "mrv");
        }

        public MemoryStream WriteToMrvWithAllRoute(string result)
        {
            //取根路线
            List<TreeNodes> treeNodes = JsonConvert.DeserializeObject<List<TreeNodes>>(result);
            TreeNodes rootTree = new TreeNodes();
            foreach (var item in treeNodes)
            {
                if (item.PID == 0)
                {
                    rootTree = item;
                }
            }
            //树绘图
            MDocument md = WriteTree(rootTree);
            MemoryStream stream = new MemoryStream(MolExporter.exportToBinFormat(md, "cdx"));
            return stream;
        }

        /// <summary>
        /// 绘图
        /// </summary>
        /// <param name="rootTree"></param>
        /// <returns></returns>
        private MDocument WriteTree(TreeNodes rootTree)
        {
            MoleCulePostion mp = new MoleCulePostion(rootTree.Smiles);
            MDocument md = new MDocument(mp.Mol);
            //初始化树的叶子节点数量
            InitTreeLeapNum(rootTree);

            //开始遍历子节点
            _DicListTextPosition = new Dictionary<int, List<string>>();
            _DicListTextPosition.Add(0, new List<string>());//上
            _DicListTextPosition.Add(1, new List<string>());//中
            _DicListTextPosition.Add(2, new List<string>());//下
            SearchChildNode(rootTree, md, mp, mp.Mol, 1);
            return md;
        }

        /// <summary>
        /// 初始化树的叶子节点数量
        /// </summary>
        /// <param name="rootTree"></param>
        /// <returns></returns>
        private void InitTreeLeapNum(TreeNodes treeNode)
        {
            int num = 0;
            foreach (var item in treeNode.Children)
            {
                if (item.Children.Count == 0)
                {
                    item.ChildLeapNum = 1;
                }
                else
                {
                    InitTreeLeapNum(item);
                }
                num += item.ChildLeapNum;
            }
            treeNode.ChildLeapNum = num;
        }

        /// <summary>
        /// 遍历子节点加入画图中
        /// </summary>
        /// <param name="node">当前节点</param>
        /// <param name="md">画图对象</param>
        /// <param name="previousMp">前一个节点的位置坐标对象</param>
        /// <param name="RootMol">根mol</param>
        /// <param name="level">层级</param>
        private void SearchChildNode(TreeNodes node, MDocument md, MoleCulePostion previousMp, Molecule RootMol, int bracnchIndex)
        {
            double molLeftSpace = 0;
            int num = node.Children.Count;
            if (num == 1)
            {
                TreeNodes childNode = node.Children[0];
                //绘制
                MoleCulePostion mp = new MoleCulePostion(childNode.Smiles);
                molLeftSpace = mp.Center_x - mp.Left_x;
                SetMolPosition(mp, previousMp.Right_x + SpanceX + molLeftSpace, previousMp.Center_y);
                RootMol.fuse(mp.Mol);
                //写mol层次号
                string txt = ReactionSteps.ToString() + "a";
                _DicListTextPosition[bracnchIndex].Add(txt + "," + mp.Center_x + "," + (mp.Bottom_y - TxtspaceWithMolInY));

                //划水平线
                MPoint p1 = new MPoint(previousMp.Right_x + SpanceX * (0.5 - SpaceLineScale / 2), previousMp.Center_y);
                MPoint p2 = new MPoint(previousMp.Right_x + SpanceX * (0.5 + SpaceLineScale / 2), previousMp.Center_y);
                MRectangle arrow = new MRectangle(p1, p2);
                md.addObject(arrow);
                //写反应步次
                md.addObject(CreateMTextBox((ReactionSteps++).ToString(),
                    new MPoint(previousMp.Right_x + SpanceX * 0.5, previousMp.Center_y + ReactionIndexSpaceWithLine)));

                //遍历子集
                SearchChildNode(childNode, md, mp, RootMol, bracnchIndex);
            }
            else if (num > 1)
            {
                WriteText(md, bracnchIndex);
                if (num % 2 == 0)
                {
                    //对称式绘出节点位置
                    double topY = previousMp.Center_y;
                    double bottomY = previousMp.Center_y;
                    List<double> listHigh = new List<double>();
                    List<char> listChar = new List<char>();
                    for (int i = 0; i < num; i++)
                    {
                        listChar.Add((char)('a' + i));
                    }
                    var list = node.Children;
                    for (int i = 0; i < list.Count; i += 2)
                    {
                        //记录当前反应的步数
                        int curStep = ReactionSteps++;
                        double tempY1 = list[i].ChildLeapNum * SpanceY;
                        double tempY2 = list[i + 1].ChildLeapNum * SpanceY;

                        topY += tempY1 > tempY2 ? tempY1 : tempY2;
                        bottomY -= tempY1 > tempY2 ? tempY1 : tempY2;
                        listHigh.Add(topY);
                        listHigh.Add(bottomY);
                        //上半部分
                        MoleCulePostion mp1 = new MoleCulePostion(list[i].Smiles);
                        molLeftSpace = mp1.Center_x - mp1.Left_x;
                        SetMolPosition(mp1, previousMp.Right_x + SpanceX + molLeftSpace, topY);
                        RootMol.fuse(mp1.Mol);
                        //写mol层次号
                        string txt1 = curStep.ToString() + listChar[num - (num / 2 + i / 2) - 1];
                        _DicListTextPosition[0].Add(txt1 + "," + mp1.Center_x + "," + (mp1.Bottom_y - TxtspaceWithMolInY));
                        //找子集
                        SearchChildNode(list[i], md, mp1, RootMol, 0);

                        //下半部分
                        MoleCulePostion mp2 = new MoleCulePostion(list[i + 1].Smiles);
                        molLeftSpace = mp2.Center_x - mp2.Left_x;
                        SetMolPosition(mp2, previousMp.Right_x + SpanceX + molLeftSpace, bottomY);
                        RootMol.fuse(mp2.Mol);
                        //写mol层次号
                        string txt2 = curStep.ToString() + listChar[num / 2 + i / 2];
                        _DicListTextPosition[2].Add(txt2 + "," + mp2.Center_x + "," + (mp2.Bottom_y - TxtspaceWithMolInY));
                        //写反应步次
                        double startWithText = 0.5 - SpaceLineScale / 4;
                        md.addObject(CreateMTextBox((curStep).ToString(),
                            new MPoint(previousMp.Right_x + SpanceX * startWithText, previousMp.Center_y + ReactionIndexSpaceWithLine)));
                        WriteBranchLine(previousMp.Right_x, previousMp.Center_y, listHigh, md);

                        //找子集
                        SearchChildNode(list[i + 1], md, mp2, RootMol, 2);
                    }

                }
                else
                {
                    //记录当前反应的步数
                    int curStep = ReactionSteps++;
                    double topY = previousMp.Center_y;
                    double bottomY = previousMp.Center_y;
                    List<double> listHigh = new List<double>();
                    List<char> listChar = new List<char>();
                    for (int i = 0; i < num; i++)
                    {
                        listChar.Add((char)('a' + i));
                    }
                    var list = node.Children;
                    //先绘画水平节点
                    MoleCulePostion mp = new MoleCulePostion(list[0].Smiles);
                    molLeftSpace = mp.Center_x - mp.Left_x;
                    SetMolPosition(mp, previousMp.Right_x + SpanceX + molLeftSpace, previousMp.Center_y);
                    RootMol.fuse(mp.Mol);
                    //写mol层次号
                    string txt = curStep.ToString() + listChar[num / 2];
                    _DicListTextPosition[1].Add(txt + "," + mp.Center_x + "," + (mp.Bottom_y - TxtspaceWithMolInY));
                    //找子集
                    SearchChildNode(list[0], md, mp, RootMol, 1);
                    //分隔两侧间距
                    double high_mp = list[0].ChildLeapNum * SpanceY;
                    topY += high_mp;
                    bottomY -= high_mp;
                    listHigh.Add(previousMp.Center_y);
                    //两端节点
                    for (int i = 1; i < list.Count; i += 2)
                    {
                        double tempY1 = list[i].ChildLeapNum * SpanceY;
                        double tempY2 = list[i + 1].ChildLeapNum * SpanceY;

                        topY += tempY1 > tempY2 ? tempY1 : tempY2;
                        bottomY -= tempY1 > tempY2 ? tempY1 : tempY2;
                        listHigh.Add(topY);
                        listHigh.Add(bottomY);
                        //上半部分
                        MoleCulePostion mp1 = new MoleCulePostion(list[i].Smiles);
                        molLeftSpace = mp1.Center_x - mp1.Left_x;
                        SetMolPosition(mp1, previousMp.Right_x + SpanceX + molLeftSpace, topY);
                        RootMol.fuse(mp1.Mol);
                        //写mol层次号
                        string txt1 = curStep.ToString() + listChar[num / 2 - 1 - i / 2];
                        _DicListTextPosition[0].Add(txt1 + "," + mp1.Center_x + "," + (mp1.Bottom_y - TxtspaceWithMolInY));
                        //找子集
                        SearchChildNode(list[i], md, mp1, RootMol, 0);

                        //下半部分
                        MoleCulePostion mp2 = new MoleCulePostion(list[i + 1].Smiles);
                        molLeftSpace = mp2.Center_x - mp2.Left_x;
                        SetMolPosition(mp2, previousMp.Right_x + SpanceX + molLeftSpace, bottomY);
                        RootMol.fuse(mp2.Mol);
                        //写mol层次号
                        string txt2 = curStep.ToString() + listChar[num / 2 + 1 + i / 2];
                        _DicListTextPosition[2].Add(txt2 + "," + mp2.Center_x + "," + (mp2.Bottom_y - TxtspaceWithMolInY));

                        //写反应步次
                        double startWithText = 0.5 - SpaceLineScale / 4;
                        md.addObject(CreateMTextBox((curStep).ToString(), new MPoint(previousMp.Right_x + SpanceX * startWithText, previousMp.Center_y + ReactionIndexSpaceWithLine)));
                        WriteBranchLine(previousMp.Right_x, previousMp.Center_y, listHigh, md);
                        //找子集
                        SearchChildNode(list[i + 1], md, mp2, RootMol, 2);
                    }
                }
            }
            else
            {
                WriteText(md, bracnchIndex);
            }
        }

        private void WriteText(MDocument md, int bracnchIndex)
        {
            double max = 9999999;
            foreach (var item in _DicListTextPosition[bracnchIndex])
            {
                var list = item.Split(',').ToList();
                double y = Convert.ToDouble(list[2]);
                if (max > y)
                {
                    max = y;
                }
            }
            foreach (var item in _DicListTextPosition[bracnchIndex])
            {
                var list = item.Split(',').ToList();
                double x = Convert.ToDouble(list[1]);
                md.addObject(CreateMTextBox(list[0], new MPoint(x, max)));
            }
            _DicListTextPosition[bracnchIndex] = new List<string>();
        }

        /// <summary>
        /// 偶数不变，奇数，根据参数加1或减1
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double ChangeValue(double value, bool tag)
        {
            if (tag)
            {
                return value % 2 == 0 ? value : value + 1;
            }
            else
            {
                return value % 2 == 0 ? value : value - 1;
            }
        }

        /// <summary>
        /// 根据最后一个分子，以及分支数量，划分支线
        /// </summary>
        /// <param name="right_x"></param>
        /// <param name="center_y"></param>
        /// <param name="num"></param>
        private void WriteBranchLine(double right_x, double center_y, List<double> listHigh, MDocument md)
        {
            //水平线部分
            MPoint p1 = new MPoint(right_x + SpanceX * (0.5 - SpaceLineScale / 2), center_y);
            MPoint p2 = new MPoint(right_x + SpanceX * 0.5, center_y);
            MRectangle arrow1 = new MRectangle(p1, p2);
            md.addObject(arrow1); ;
            //划竖直线
            double topy = -9999999;
            double bottomy = 9999999;
            foreach (var item in listHigh)
            {
                topy = topy < item ? item : topy;
                bottomy = bottomy > item ? item : bottomy;
            }
            MPoint p3 = new MPoint(right_x + SpanceX * 0.5, topy);
            MPoint p4 = new MPoint(right_x + SpanceX * 0.5, bottomy);
            MRectangle arrow2 = new MRectangle(p3, p4);
            md.addObject(arrow2);
            //根据传入的高度记录划水平分支线
            foreach (double high in listHigh)
            {
                MPoint p5 = new MPoint(right_x + SpanceX * 0.5, high);
                MPoint p6 = new MPoint(right_x + SpanceX * (0.5 + SpaceLineScale / 2), high);
                MRectangle arrow3 = new MRectangle(p5, p6);
                md.addObject(arrow3);
            }
        }
        #endregion

        #region 划分支相关代码
        /// <summary>
        /// 返回string格式的mrv
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public string WriteToMrvWithBranchWithStr(string result)
        {
            //取根路线
            List<TreeNodes> treeNodes = JsonConvert.DeserializeObject<List<TreeNodes>>(result);
            TreeNodes rootTree = new TreeNodes();
            foreach (var item in treeNodes)
            {
                if (item.PID == 0)
                {
                    rootTree = item;
                }
            }
            //树绘图
            MDocument md = WriteBrachTree(rootTree);
            return MolExporter.exportToFormat(md, "mrv");
        }

        /// <summary>
        /// 返回文件流的mrv
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public MemoryStream WriteToMrvWithBranch(string result)
        {
            //取根路线
            List<TreeNodes> treeNodes = JsonConvert.DeserializeObject<List<TreeNodes>>(result);
            TreeNodes rootTree = new TreeNodes();
            foreach (var item in treeNodes)
            {
                if (item.PID == 0)
                {
                    rootTree = item;
                }
            }
            //树绘图
            MDocument md = WriteBrachTree(rootTree);
            MemoryStream stream = new MemoryStream(MolExporter.exportToBinFormat(md, "cdx"));
            return stream;
        }

        /// <summary>
        /// 绘图
        /// </summary>
        /// <param name="rootTree"></param>
        /// <returns></returns>
        private MDocument WriteBrachTree(TreeNodes rootTree)
        {
            //获取所有分支
            List<string> listBranch = GetAllBranch(rootTree);

            //开始遍历子节点
            _ListTextPosition = new List<string>();
            MDocument md = WriteBranchWithMD(listBranch);
            return md;
        }

        /// <summary>
        /// 根据搜索好的路线绘制分支线
        /// </summary>
        /// <param name="listBranch"></param>
        /// <param name="mp"></param>
        /// <param name="md"></param>
        private MDocument WriteBranchWithMD(List<string> listBranch)
        {
            //当前行
            int lineIndex = 0;
            //先绘制出一条线
            List<string> branchList = listBranch[0].Split(',').ToList();
            var molList = branchList[0].Split(';').ToList();
            MoleCulePostion mp = new MoleCulePostion(molList[0]);
            MDocument md = new MDocument(mp.Mol);
            //写mol层次号
            string txt = molList[1];
            _ListTextPosition.Add(txt + "," + mp.Center_x + "," + (mp.Bottom_y - TxtspaceWithMolInY));
            WriteSimpleBranch(mp, branchList, 1, mp.Mol, md, true);
            lineIndex += branchList.Count / BranchOneLineNum + (branchList.Count % BranchOneLineNum > 0 ? 1 : 0);

            //再绘制出其他的线
            for (int i = 1; i < listBranch.Count; i++)
            {
                List<string> branchList1 = listBranch[i].Split(',').ToList();
                var molList1 = branchList1[0].Split(';').ToList();
                MoleCulePostion mp1 = new MoleCulePostion(molList1[0]);
                SetMolPosition(mp1, 0, mp.Center_y - SpanceY * BranchSPFWithY * lineIndex);
                //写mol层次号
                txt = molList1[1];
                _ListTextPosition.Add(txt + "," + mp1.Center_x + "," + (mp1.Bottom_y - TxtspaceWithMolInY));
                lineIndex += branchList1.Count / BranchOneLineNum + (branchList1.Count % BranchOneLineNum > 0 ? 1 : 0);
                //划分支
                WriteSimpleBranch(mp1, branchList1, 1, mp.Mol, md, false);
                //合并分支显示
                mp.Mol.fuse(mp1.Mol);
            }
            //返回绘图
            return md;
        }

        /// <summary>
        /// 不包括根点的树单分支递归绘制
        /// </summary>
        /// <param name="previousMp">前一个mol类</param>
        /// <param name="branchList">分支smiles记录</param>
        /// <param name="index">当前次序</param>
        /// <param name="rootMol">根mol</param>
        /// <param name="md">文档对象</param>
        /// <param name="isRoot">是否是最长分支路线</param>
        private void WriteSimpleBranch(MoleCulePostion previousMp, List<string> branchList, int index, Molecule rootMol, MDocument md, bool isRoot)
        {
            double molLeftSpace = 0;
            if (branchList.Count <= index)
            {
                //WriteText(md);
                return;
            }
            double curY = previousMp.Center_y;
            double curX = previousMp.Right_x;
            if (index % BranchOneLineNum == 0)
            {
                curY -= SpanceY * BranchSPFWithY;
                curX = 0 - SpanceX * BranchSPFWithX;
                //WriteText(md);
            }
            //写反应的其他物质，仅限最长路线
            int anotherMolNum = 0;
            double maxLength = 0;
            if (!string.IsNullOrEmpty(DicBranchSmiles[index]) && isRoot)
            {
                List<string> listSmiles = DicBranchSmiles[index].Split(',').ToList();
                anotherMolNum = listSmiles.Count;
                if (listSmiles.Count == 1)
                {
                    var molList = listSmiles[0].Split(';').ToList();
                    MoleCulePostion mp1 = new MoleCulePostion(molList[0]);
                    double leftDistance = mp1.Center_x - mp1.Left_x;
                    double bottomDistance = mp1.Center_y - mp1.Bottom_y;
                    maxLength = mp1.Right_x - mp1.Left_x + BranchAnotherMolDistanceX * 2;
                    SetMolPosition(mp1, previousMp.Right_x + leftDistance + BranchAnotherMolDistanceX,
                        previousMp.Center_y + bottomDistance + BranchAnotherMolDistanceY + 1.5);
                    //写箭头上mol层次号
                    md.addObject(CreateMTextBox(molList[1],
                        new MPoint(previousMp.Right_x + leftDistance + BranchAnotherMolDistanceX,
                        previousMp.Center_y + 1.5)));
                    rootMol.fuse(mp1.Mol);
                }
                else if (listSmiles.Count == 2)
                {
                    foreach (var smiles in listSmiles)
                    {
                        var molList = smiles.Split(';').ToList();
                        MoleCulePostion mp1 = new MoleCulePostion(molList[0]);
                        double leftDistance = mp1.Center_x - mp1.Left_x;
                        double bottomDistance = mp1.Center_y - mp1.Bottom_y;
                        SetMolPosition(mp1, maxLength + previousMp.Right_x + leftDistance + BranchAnotherMolDistanceX,
                            previousMp.Center_y + bottomDistance + BranchAnotherMolDistanceY + 1.5);
                        //写箭头上mol层次号
                        md.addObject(CreateMTextBox(molList[1],
                            new MPoint(maxLength + previousMp.Right_x + leftDistance + BranchAnotherMolDistanceX,
                            previousMp.Center_y + 1.5)));
                        rootMol.fuse(mp1.Mol);
                        maxLength += mp1.Right_x - mp1.Left_x + BranchAnotherMolDistanceX * 2;
                    }
                }
            }
            //箭头绘制;
            MPoint p1 = new MPoint(previousMp.Right_x + SpanceX * BranchSPFWithX * 0.15, previousMp.Center_y);
            MPoint p2 = new MPoint(previousMp.Right_x + SpanceX * BranchSPFWithX * 0.85, previousMp.Center_y);
            if (anotherMolNum == 2)
            {
                p1 = new MPoint(previousMp.Right_x + maxLength * 0.15, previousMp.Center_y);
                p2 = new MPoint(previousMp.Right_x + maxLength * 0.85, previousMp.Center_y);
            }
            if (anotherMolNum == 1)
            {
                p1 = new MPoint(previousMp.Right_x + maxLength * 0.15, previousMp.Center_y);
                p2 = new MPoint(previousMp.Right_x + maxLength * 0.85, previousMp.Center_y);
            }
            MPolyline arrow = new MPolyline(p1, p2);
            arrow.setArrow(true);
            arrow.setArrowLength(1, chemaxon.struc.graphics.MPolyline.DEFAULT_ARROW_HEAD_LENGTH / 2);
            arrow.setArrowWidth(1, chemaxon.struc.graphics.MPolyline.DEFAULT_ARROW_HEAD_WIDTH);
            arrow.setArrowFlags(1, chemaxon.struc.graphics.MPolyline.ARROW_DASHED_FLAG);
            md.addObject(arrow);
            //分子绘制
            var molList1 = branchList[index].Split(';').ToList();
            MoleCulePostion tempMp = new MoleCulePostion(molList1[0]);
            molLeftSpace = tempMp.Center_x - tempMp.Left_x;
            if (curX == -SpanceX * BranchSPFWithX)
            {
                molLeftSpace = 0;
                maxLength = SpanceX * BranchSPFWithX;
            }
            if (anotherMolNum == 2)
            {
                SetMolPosition(tempMp, curX + maxLength + molLeftSpace, curY);
            }
            else if (anotherMolNum == 1)
            {
                SetMolPosition(tempMp, curX + maxLength + molLeftSpace, curY);
            }
            else if (anotherMolNum == 0)
            {
                SetMolPosition(tempMp, curX + SpanceX * BranchSPFWithX + molLeftSpace, curY);
            }
            rootMol.fuse(tempMp.Mol);
            //写mol层次号
            string txt = molList1[1];
            if (txt != "0a")
            {
                _ListTextPosition.Add(txt + "," + tempMp.Center_x + "," + (tempMp.Bottom_y - TxtspaceWithMolInY));
            }
            //下一级
            WriteSimpleBranch(tempMp, branchList, index + 1, rootMol, md, isRoot);
        }

        //获取所有分支
        private List<string> GetAllBranch(TreeNodes rootTree)
        {
            //获取所有的线
            List<string> mrvBranch = new List<string>();
            InitTreeWithDepth(rootTree);
            SearchAllBranch(rootTree, mrvBranch, 'a', 0);
            SortByLengthWithList(mrvBranch);
            return mrvBranch;
        }

        /// <summary>
        /// 初始化树所有节点的长度
        /// </summary>
        /// <param name="rootTree"></param>
        private void InitTreeWithDepth(TreeNodes node)
        {
            if (node.Children.Count == 0)
            {
                node.MaxDepth = 1;
                return;
            }
            int childDept = -1;
            foreach (var item in node.Children)
            {
                InitTreeWithDepth(item);
                if (item.MaxDepth > childDept)
                    childDept = item.MaxDepth;
            }
            node.MaxDepth = 1 + childDept;
        }

        /// <summary>
        /// 对分支排序由长至短
        /// </summary>
        /// <param name="mrvBranch"></param>
        private void SortByLengthWithList(List<string> mrvBranch)
        {
            for (int i = 0; i < mrvBranch.Count; i++)
            {
                int num = mrvBranch[i].Split(',').Length;
                for (int j = i + 1; j < mrvBranch.Count; j++)
                {
                    int tempNum = mrvBranch[j].Split(',').Length;
                    if (num < tempNum)
                    {
                        string str = mrvBranch[i];
                        mrvBranch[i] = mrvBranch[j];
                        mrvBranch[j] = str;
                    }
                }
            }
        }
        /// <summary>
        /// 找到所有不存在重复分子的分支
        /// </summary>
        /// <param name="node"></param>
        /// <param name="mrvBranch"></param>
        private void SearchAllBranch(TreeNodes node, List<string> mrvBranch, char index, int level)
        {
            if (node.IsSearch == false)
            {
                //找当前节点的最长路线
                string route = GetMaxLengthRoute(node, node.PID == 0 ? true : false, index, level);
                if (route.Split(',').Count() > 1)
                    mrvBranch.Add(route);
                node.IsSearch = true;
            }
            int i = 0;
            foreach (var item in node.Children)
            {
                SearchAllBranch(item, mrvBranch, Convert.ToChar('a' + i++), level + 1);
            }
        }

        /// <summary>
        /// 找当前节点的最长路线(反向输出路线)
        /// </summary>
        /// <param name="node"></param>
        /// <param name="isRoots">用于判断当前分支是否是最长分支搜索</param>
        /// <returns></returns>
        private string GetMaxLengthRoute(TreeNodes node, bool isRoots, char index, int level)
        {
            node.IsSearch = true;
            if (node.Children.Count == 0)
            {
                return node.Smiles + ";" + level + index;
            }
            int dept = -1;
            TreeNodes temp = new TreeNodes();
            int i = 0;
            int tag_i = 0;
            foreach (var item in node.Children)
            {
                if (dept < item.MaxDepth)
                {
                    dept = item.MaxDepth;
                    temp = item;
                    tag_i = i;
                }
                i++;
            }
            if (isRoots)
            {
                string str = string.Empty;
                int j = 0;
                foreach (var item in node.Children)
                {
                    if (item.NodeID != temp.NodeID)
                    {
                        if (string.IsNullOrEmpty(str))
                            str += item.Smiles + ";" + (level + 1) + Convert.ToChar('a' + j);
                        else
                            str += "," + item.Smiles + ";" + (level + 1) + Convert.ToChar('a' + j);
                    }
                    j++;
                }
                if (DicBranchSmiles == null)
                    DicBranchSmiles = new Dictionary<int, string>();
                DicBranchSmiles.Add(temp.MaxDepth, str);
            }
            char childIndex = Convert.ToChar('a' + tag_i);
            return GetMaxLengthRoute(temp, isRoots, childIndex, level + 1) + "," + node.Smiles + ";" + level + index;
        }
        #endregion

        /// <summary>
        /// 设置每个原子距离原点的X距离，Y距离
        /// </summary>
        /// <param name="mol2"></param>
        /// <param name="spaceX"></param>
        /// <param name="spaceY"></param>
        private void SetMolPosition(MoleCulePostion mp, double spaceX, double spaceY)
        {
            foreach (var item in mp.Mol.getAtomArray())
            {
                double x = item.getX();
                double y = item.getY();
                item.setX(x + spaceX);
                item.setY(y + spaceY);
            }
            //调用位置类计算位置函数重新赋值
            mp.AddSpace(spaceX, spaceY);
        }

        private MTextBox CreateMTextBox(string text, MPoint p1)
        {
            MTextBox molText = new MTextBox();
            molText.setText(text);
            molText.setHorizontalAlignment(MTextBox.ALIGN_LEFT);
            molText.setCorners(p1, p1);
            molText.setAutoSize(true);
            return molText;
        }

        private class MoleCulePostion
        {
            public double Top_y { get; set; }
            public double Bottom_y { get; set; }
            public double Left_x { get; set; }
            public double Right_x { get; set; }
            public double Center_x { get; set; }
            public double Center_y { get; set; }
            public Molecule Mol { get; set; }

            public MoleCulePostion(string smiles)
            {
                Mol = MolImporter.importMol(smiles);
                //将苯环的圈改为双线
                Mol.dearomatize();
                chemaxon.calculations.clean.Cleaner.clean(Mol, 2);

                //重置类参数
                InitPosition();
                //重置原子垂直坐标
                InitAtom();
            }

            private void InitAtom()
            {
                double difX = -Center_x;
                double difY = -Center_y;
                foreach (var item in Mol.getAtomArray())
                {
                    double x = item.getX();
                    double y = item.getY();
                    item.setX(x + difX);
                    item.setY(y + difY);
                }
                AddSpace(difX, difY);
            }

            /// <summary>
            /// 获取每个原子的中心坐标
            /// </summary>
            /// <param name="mol"></param>
            /// <returns></returns>
            public void InitPosition()
            {
                double top_y = -9999999;
                double bottom_y = 9999999;
                double right_x = -9999999;
                double left_x = 9999999;

                foreach (var item in this.Mol.getAtomArray())
                {
                    double x = item.getX();
                    double y = item.getY();

                    top_y = top_y < y ? y : top_y;
                    bottom_y = bottom_y > y ? y : bottom_y;
                    right_x = right_x < x ? x : right_x;
                    left_x = left_x > x ? x : left_x;
                }

                double c_y = (top_y + bottom_y) / 2;
                double c_x = (left_x + right_x) / 2;

                //位置值初始化
                Top_y = top_y;
                Bottom_y = bottom_y;
                Left_x = left_x;
                Right_x = right_x;
                Center_x = c_x;
                Center_y = c_y;
            }

            public void AddSpace(double x, double y)
            {
                this.Top_y += y;
                this.Bottom_y += y;
                this.Left_x += x;
                this.Right_x += x;
                this.Center_x = (this.Left_x + this.Right_x) / 2;
                this.Center_y = (this.Top_y + this.Bottom_y) / 2;
            }
        }
    }
}
