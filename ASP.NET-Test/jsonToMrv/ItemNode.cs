using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace jsonToMrv
{
    [Serializable]
    public class ItemNode
    {
        public int PID { set; get; }
        public int NodeID { set; get; }
        public double Weight { set; get; }
        public double Yield { set; get; }
        public string Smiles { set; get; }
        public string CAS { set; get; }
        public string Name { set; get; }
        public decimal NeedMoney { set; get; }
    }

    public class QuerySmilesByCAS
    {
        public string Smiles { set; get; }
        public string CAS { set; get; }
    }

    public class TreeNodes
    {
        public int level;
        private TreeNodes father;
        public List<string> searchedSmiles = new List<string>();
        public List<TreeNodes> Children = new List<TreeNodes>();
        public Reaction reaction;
        public int ComboRetroReactionID;
        public List<int> maps;
        public int Feasibility;
        public string Reference;
        public string Procedure;
        public string Reagent;
        public string Solvent;
        public string Catalyst;
        public string Condition;
        public int prodcdid;
        public int RetroReactionID;
        //public decimal MolPrice;
        public List<int> ComboRetroReactionIDs;
        public List<int> RetroReactionIDs;
        public string testString;
        public int ChoiseRank;
        public int RankIndexSimil;
        public int RankIndexScore;
        public double fiabilityScore;
        public bool needProtection;
        public int reactionId;
        /// <summary>
        /// 当前节点的叶子数量
        /// </summary>
        public int ChildLeapNum;
        /// <summary>
        /// 节点遍历时是否遍历过
        /// </summary>
        public bool IsSearch = false;
        /// <summary>
        /// 当前节点的最大长度
        /// </summary>
        public int MaxDepth;

        //public string filename;

        //public string MainReactifSmiles;

        public TreeNodes()
        { }
        public TreeNodes(TreeNodes fatherNode, Reaction react)
        {
            //NodeID = nodeID;
            Smiles = react.smilesInCode;
            if (fatherNode != null)
            {
                father = fatherNode;
                //PID = father.NodeID;
                level = father.level + 1;
                searchedSmiles = father.searchedSmiles.ToList();
                father.Children.Add(this);
            }
            reaction = react;
            CAS = react.cas;
            //MolPrice = react.MolPrice;
            //reaction = new ReturnList();
            //reaction.reactionObject = new List<Reaction>();
            //Reaction re = react;
            ////re.smilesInCode = react;
            //reaction.reactionObject.Add(re);
            searchedSmiles.Add(Smiles);

        }

        public int PID { set; get; }//父节点的ID
        public int NodeID { set; get; }//该节点的ID
        public string Yield { set; get; }//产率
        public string Smiles { set; get; }
        public string CAS { set; get; }
        public int HasPrice { set; get; }//是否有报价
        public int CitationID { set; get; }//条件ID

        //计算
        public decimal Price { get; set; }
        public decimal Package { set; get; }
        public string Unit { get; set; }
        public string CurrencyType { set; get; }

        public string Purity { set; get; }

        public string GenerateTreeNodesList(List<TreeNodes> list, ref int nodeid, int level)
        {
            //if (mainReactifs != null && Children != null && Children.Count > 0)
            //{
            //    string mainreact = Children.First(chi => chi.Smiles.Length == Children.Max(ch => ch.Smiles.Length)).Smiles;
            //    mainReactifs.Add(mainreact);
            //}
            string nodeString = "|" + Smiles + "|";
            string newnodeString = nodeString;
            list.Add(this);
            NodeID = nodeid;
            foreach (var child in Children)
            {
                child.PID = NodeID;
                if (child.reaction.MolPrice > 0)
                    child.reaction.YieldPrice = child.reaction.MolPrice / (decimal)Math.Pow(0.9, level);
                nodeid++;
                string childnodes = child.GenerateTreeNodesList(list, ref nodeid, level + 1);
                if (childnodes.Contains(nodeString))
                    throw new Exception("node repeated");
                newnodeString += childnodes;
            }
            return newnodeString;
        }

        public string GenerateTreeStepTag(int level)
        {
            if (level == 0)
                return "";
            string nodeString = "|" + Smiles + "|";
            string newnodeString = nodeString;
            foreach (var child in Children.OrderBy(ch => ch.Smiles))
            {
                string childnodes = child.GenerateTreeStepTag(level - 1);
                newnodeString += childnodes;
            }
            return newnodeString;
        }
    }

    public class Reaction
    {
        public string otherSmiles;

        public string smiles
        {
            set
            {
                smilesInCode = value;
            }
            get
            {
                return smilesInCode;

            }
        }

        public string smilesInCode;
        public bool FindWay;
        //public double bestStepPrice;
        public Node BestChildNode;

        public bool isMainReactif;
        public List<int> ids;
        public List<Retro> retros;
        internal Node currentChildNode;
        public bool knownMol;

        //public bool hasMoleculeID;


        public string cas { set; get; }

        public int chemicalID { set; get; }


        public double molMass { set; get; }

        //[DataMember]
        //public decimal price { set; get; }
        //[DataMember]
        //public string unit { set; get; }

        public int hasPrice { set; get; }

        public int canMake { set; get; }
        public int HasSupplier { set; get; }
        public decimal MolPrice { set; get; }
        public decimal YieldPrice { set; get; }

        public int MoleculeID { set; get; }
    }

    public class Node
    {
        public string target;
        public List<string> searchedSmiles = new List<string>();
        public List<string> steps = new List<string>();
        public ReturnList reaction;
        //internal bool havePendingBranch;
        public Node father;
        public string fatherproduct;
        public string currentSmiles;
        public List<Reaction> reactList;
        public Reaction targetReactif;
        public decimal newStepPrice;
        public int canMake;
        public int Score;
        //public int DoubleScore;
        public int ChoiseRank;
        public Node()
        {
        }

        public Node(string rootProduct)
        {
            currentSmiles = rootProduct;
            target = rootProduct;
            targetReactif = new Reaction();
            targetReactif.smilesInCode = target;
            reaction = new ReturnList();
            reaction.reactionObject = new List<Reaction>();
            reaction.reactionObject.Add(new Reaction() { canMake = 0, smilesInCode = target, isMainReactif = true });
            steps.Add(target);
            searchedSmiles.Add(target);
            targetReactif.isMainReactif = true;
            reactList = reaction.reactionObject;
        }
        public Node(Node fatherNode, string react)
        {
            currentSmiles = react;
            if (fatherNode != null)
            {
                father = fatherNode;
                level = father.level + 1;
                searchedSmiles = father.searchedSmiles.ToList();
                father.Children.Add(this);
            }
            else
            {
                Score = react.Length;
            }
            reaction = new ReturnList();
            reaction.reactionObject = new List<Reaction>();
            Reaction re = new Reaction();
            re.smilesInCode = react;
            reaction.reactionObject.Add(re);
            searchedSmiles.Add(react);
        }
        public Node createChild()
        {
            Node newNode = new Node();
            newNode.steps = steps.ToList();
            newNode.searchedSmiles = searchedSmiles.ToList();
            newNode.fatherproduct = target;
            newNode.father = this;
            newNode.Score = Score;
            newNode.RankScore = RankScore;
            Children.Add(newNode);
            return newNode;
        }

        public string GeneratePlan()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < steps.Count; i++)
            {
                sb.Append(steps[i] + ".O.");
            }
            sb = sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        /// <summary>
        /// 广度优先遍历树并生成图片string和每一步反应smiles
        /// </summary>
        /// <param name="smilesDetails"></param>
        /// <returns></returns>
        public string GenerateNewPlan(out List<string> smilesDetails)
        {
            smilesDetails = new List<string>();
            StringBuilder sb = new StringBuilder();
            ConcurrentQueue<Node> queue = new ConcurrentQueue<Node>();
            queue.Enqueue(this);
            while (queue.Count != 0)
            {
                int queueCount = queue.Count;
                for (int queuei = 0; queuei < queueCount; queuei++)
                {
                    Node currentNode = null;
                    if (!queue.TryDequeue(out currentNode))
                        continue;
                    string reactionString = "";
                    foreach (var react in currentNode.reaction.reactionObject)
                    {
                        reactionString += react.smilesInCode + ".";
                        sb.Append(react.smilesInCode + ".");
                        if (react.BestChildNode != null)
                            queue.Enqueue(react.BestChildNode);
                    }
                    if (currentNode.father != null)
                    {
                        reactionString = reactionString.Remove(reactionString.Length - 1, 1) + ">>" + currentNode.targetReactif.smilesInCode;
                        smilesDetails.Add(reactionString);
                    }
                }
                if (queue.Count != 0)
                {
                    sb.Append("O.");
                }
            }
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        public string GenerateNewPlanForSimil(out List<string> smilesDetails)
        {
            smilesDetails = new List<string>();
            StringBuilder sb = new StringBuilder();
            ConcurrentQueue<Node> queue = new ConcurrentQueue<Node>();
            queue.Enqueue(this);
            while (queue.Count != 0)
            {
                int queueCount = queue.Count;
                for (int queuei = 0; queuei < queueCount; queuei++)
                {
                    Node currentNode = null;
                    if (!queue.TryDequeue(out currentNode))
                        continue;
                    string reactionString = "";
                    foreach (var react in currentNode.reaction.reactionObject)
                    {
                        reactionString += react.smilesInCode + ".";
                        sb.Append(react.smilesInCode + ".");

                    }
                    foreach (Node child in currentNode.Children)
                        queue.Enqueue(child);
                    if (currentNode.father != null)
                    {
                        reactionString = reactionString.Remove(reactionString.Length - 1, 1) + ">>" + currentNode.father.currentSmiles;
                        smilesDetails.Add(reactionString);
                    }
                }
                if (queue.Count != 0)
                {
                    sb.Append("O.");
                }
            }
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
        public List<Node> Children = new List<Node>();
        public int negtifscore;
        public Node level3root;
        public bool stopSearch;
        public int level;
        public int RankScore;
        public int RankIndexSimil;
        public int RankIndexScore;
        private bool useCurrent;
        public string tag;
        public float similRank;
        public bool reserve;
        public int groupRank;
        #region find treenodes
        // find root
        public TreeNodes GetTreeNodesWay()
        {
            if (father == null)
            {
                return this.reaction.reactionObject.First().currentChildNode.createTreeNodes();
            }
            else
            {
                useCurrent = true;
            }
            return father.GetTreeNodesWay();
        }

        // build treenodes from root
        private TreeNodes createTreeNodes()
        {
            // root
            Reaction reac = new Reaction();
            reac.canMake = targetReactif.canMake;
            reac.smilesInCode = targetReactif.smilesInCode;
            reac.knownMol = targetReactif.knownMol;
            TreeNodes treeRoot = new TreeNodes();
            treeRoot.reaction = reac;
            treeRoot.Smiles = reac.smilesInCode;
            treeRoot.Feasibility = ((reaction.HasRegioError != 0 || reaction.Retro.MayHaveError != 0) ? 1 : 0)
            + reaction.Retro.IncompatbleIDs.Count + reaction.Retro.UncertantIncompatbleIDs.Count;
            treeRoot.ComboRetroReactionID = reaction.Retro.ComboRetroReactionID;
            treeRoot.ComboRetroReactionIDs = reaction.Retro.ComboRetroReactionIDs;
            treeRoot.prodcdid = reaction.similCdid;
            treeRoot.RetroReactionID = reaction.Retro.RetroReactionID;
            treeRoot.RetroReactionIDs = reaction.Retro.RetroReactionIDs;
            treeRoot.fiabilityScore = reaction.Retro.fiabilityScore;
            treeRoot.needProtection = reaction.Retro.NeedProtection;
            treeRoot.reactionId = reaction.reactionID;
            //if (reaction.Retro.groupDetail != null && reaction.Retro.groupDetail.Count > 0)
            //{
            //    var mingroup = reaction.Retro.groupDetail.FirstOrDefault();
            //    treeRoot.testString = (((reaction.HasRegioError + reaction.Retro.MayHaveError) > 0 ? " regio" : "") + reaction.Retro.Frequency.ToString("0.0000") + " " + GroupInfo.groupNameDict[mingroup.Key] + " " + mingroup.Value.count + " " + mingroup.Value.Score.ToString("0.00"));
            //}
            //else
            //{
            //    treeRoot.testString = (((reaction.HasRegioError + reaction.Retro.MayHaveError) > 0 ? " regio" : "") + reaction.Retro.Frequency.ToString("0.0000"));
            //}
            //treeRoot.testString = reaction.similRank.ToString("0.00") + " " + reaction.Retro.fiabilityScore;//reaction.Retro.Count + " " + groupRank; //reaction.Retro.TotalCount + "-" + reaction.Retro.RetroTypeCount;//
            treeRoot.testString = ((reaction.Retro.IncompatbleIDs.Count + reaction.Retro.UncertantIncompatbleIDs.Count) > 0 ? "compat" : "") + ((reaction.HasRegioError + reaction.Retro.MayHaveError) > 0 ? " regio" : "") + ChoiseRank + "_" + RankIndexSimil + "|" + RankIndexScore;
            treeRoot.ChoiseRank = ChoiseRank;
            treeRoot.RankIndexSimil = RankIndexSimil;
            treeRoot.RankIndexScore = RankIndexScore;

            if (reaction.reactionObject != null)
            {
                foreach (var react in reaction.reactionObject)
                {
                    Node current;
                    if (react.currentChildNode != null && react.currentChildNode.useCurrent)
                    {
                        current = react.currentChildNode;
                        react.currentChildNode.useCurrent = false;
                    }
                    else
                        current = react.BestChildNode;
                    if (current == null)
                    {
                        TreeNodes treeRoot1 = new TreeNodes();
                        treeRoot1.reaction = new Reaction();
                        treeRoot1.reaction.canMake = react.canMake;
                        treeRoot1.reaction.MoleculeID = react.MoleculeID;
                        treeRoot1.reaction.smilesInCode = react.smilesInCode;
                        treeRoot1.Smiles = react.smilesInCode;
                        treeRoot1.CAS = react.cas;
                        if (treeRoot1.reaction.canMake == 1)
                            treeRoot1.reaction.MolPrice = react.MolPrice;
                        treeRoot1.reaction.HasSupplier = react.HasSupplier;
                        treeRoot1.reaction.knownMol = react.knownMol;
                        treeRoot.Children.Add(treeRoot1);
                        continue;
                    }
                    Node child = current;

                    treeRoot.Children.Add(child.createTreeNodes());
                }
            }
            return treeRoot;
        }
        #endregion

        public bool TryUpdateNodesShowAllWay()
        {
            //重新计算newStepPrice
            targetReactif.FindWay = true;
            newStepPrice = reaction.reactionObject.Sum(ro => ro.MolPrice);
            if (father == null)
            {
                // root
                return true;
            }
            if (this.reaction.Retro.ComboRetroReactionID != 0)
            {
                this.canMake = this.reaction.CanMake + this.reaction.Retro.Steps - 1;
            }
            else
            {
                this.canMake = this.reaction.CanMake;
            }
            if (targetReactif.BestChildNode == null)
            {
                targetReactif.BestChildNode = this;
            }
            else
            {
                // check step is shorter
                if (targetReactif.BestChildNode.canMake > this.canMake ||
                    (targetReactif.BestChildNode.canMake == this.canMake && targetReactif.BestChildNode.newStepPrice > newStepPrice))
                {
                    targetReactif.BestChildNode = this;
                }
            }
            targetReactif.currentChildNode = this;

            targetReactif.MolPrice = newStepPrice;
            if (!father.reactList.Any(rl => !rl.FindWay)) // 父节点所有子节点都找到终点，更新父节点状态
            {
                return father.TryUpdateNodesShowAllWay();
            }
            else
            {
                // 父节点还有节点未找到终点，不更新状态
                return false;
            }

        }
        public bool TryUpdateNodes()
        {
            //重新计算newStepPrice
            targetReactif.FindWay = true;
            newStepPrice = reaction.reactionObject.Sum(ro => ro.MolPrice);
            if (father == null)
            {
                // root
                return true;
            }
            if (targetReactif.BestChildNode == null || targetReactif.MolPrice > newStepPrice) //显示所有路线，并非最优
            {
                targetReactif.BestChildNode = this;
                targetReactif.MolPrice = newStepPrice;
                if (!father.reactList.Any(rl => !rl.FindWay)) // 父节点所有子节点都找到终点，更新父节点状态
                {
                    return father.TryUpdateNodes();
                }
                else
                {
                    // 父节点还有节点未找到终点，不更新状态
                    return false;
                }
            }
            else
            {
                ////父节点早已找到终点，价格也不用更新; 只显示主分支的变化路线
                //if (targetReactif.isMainReactif)
                //    return true;
                //else
                return false;
            }
        }
        public bool TryUpdateNodesWithMainReactif()
        {
            //重新计算newStepPrice
            targetReactif.FindWay = true;
            newStepPrice = reaction.reactionObject.Sum(ro => ro.MolPrice);
            if (father == null)
            {
                // root
                return true;
            }
            if (targetReactif.BestChildNode == null || targetReactif.MolPrice > newStepPrice) //显示所有路线，并非最优
            {
                targetReactif.BestChildNode = this;
                targetReactif.MolPrice = newStepPrice;
                if (!father.reactList.Where(rl => rl.isMainReactif).Any(rl => !rl.FindWay)) // 父节点所有子节点都找到终点，更新父节点状态
                {
                    return father.TryUpdateNodes();
                }
                else
                {
                    // 父节点还有节点未找到终点，不更新状态
                    return false;
                }
            }
            else
            {
                //父节点早已找到终点，价格也不用更新; 只显示主分支的变化路线
                if (targetReactif.isMainReactif)
                    return true;
                else
                    return false;
            }
        }


    }
    public class Retro
    {
        public int Count;
        public int TotalCount;
        public int IncreasedAtomCount;
        public int IsConvergent;
        public int RetroReactionID;
        public List<int> RetroReactionIDs;
        public string Smarts;
        public int Score;
        public int Steps;
        //public string RetroCompatibleID;
        public IEnumerable<string> ReactantSmiles;
        public string QuerySmarts;
        public int IsIntraMolecule;
        public bool IsSingleReactant;
        public bool IsDeprotection;
        //public bool HasRegioError;
        public string RetroSmiles;
        public string NoStereoRetroSmiles;
        public int Usefullness;
        //public int maxMultiRegioError;
        public int MayHaveError;
        public Dictionary<string, int> fragments;
        public int distinctLevel;
        public int Selectivity;

        public List<int> IncompatbleIDs;
        public int ErrorCode;
        public double Frequency;
        //public double Importancy;
        public int PreferScore;
        public string MainReactifSmiles;
        public string QuerySmiles;
        public List<int> UncertantIncompatbleIDs;
        public bool CheckAromPosition;
        public int bestAromaticProductid;
        public bool isQueryOrderSensitive;
        public bool isReplaceOrderSensitive;
        public bool TripleReactant;
        public int Generality;
        public int TwoStepScore;
        public int BigMolReactionCount;
        public bool HasComboRetro;
        public bool NeedProtection;
        public int CommercialScore;
        public int ComboRetroReactionID;
        public HashSet<int> Hits = new HashSet<int>();
        public int JChemQueryID;
        //public int GroupComplexity;
        public bool MultiApply;
        public bool SymetricCheck;
        //public string SimpleSmarts;
        public List<PairBond> reactedBonds;
        public Retro father;
        public bool IsSNAr;
        public string RetroSmilesWithMap;
        public int RetroTypeCount;
        //public int similCount;

        public List<int> ComboRetroReactionIDs;
        public int MultiMatch;
        public Dictionary<int, GroupInfo> groupDetail;
        public double fiabilityScore;
        public string Steric;
        public bool isHighSimilModified;
        public bool isHighSimil;
        public List<int> needProtectGroups;
        public List<int> mayNeedProtectGroups;

        public Retro Clone()
        {
            Retro newRetro = (Retro)MemberwiseClone();
            return newRetro;
        }
    }

    public class GroupInfo
    {
        public static Dictionary<int, string> groupNameDict = new Dictionary<int, string>() { { 1, "acetal" }, { 2, "acid fluoride" }, { 3, "acid chloride" }, { 4, "acid bromide" }, { 5, "acid iodide" }, { 6, "aldehyde" }, { 7, "aldoxime" }, { 8, "aliphatic amine" }, { 9, "alkene" }, { 10, "allene" }, { 11, "allyl" }, { 12, "alkoxide" }, { 13, "alkoxy" }, { 14, "alkyne" }, { 15, "amide" }, { 16, "amidine" }, { 17, "amine" }, { 18, "ammonium" }, { 19, "anhydride" }, { 20, "aryl amine" }, { 21, "azide" }, { 22, "aziridine" }, { 23, "azo" }, { 24, "azosulfone" }, { 25, "azoxy" }, { 26, "benzyl" }, { 27, "carbamate" }, { 28, "carbonate" }, { 29, "carboxylic acid" }, { 30, "carboxylate" }, { 31, "cyanimide" }, { 32, "diazo" }, { 33, "diazonium" }, { 34, "disulfide" }, { 35, "enamine" }, { 36, "enol" }, { 37, "epoxide" }, { 38, "ester" }, { 39, "ether" }, { 40, "alkyl fluoride" }, { 41, "alkyl chloride" }, { 42, "alkyl bromide" }, { 43, "alkyl iodide" }, { 44, "aryl fluoride" }, { 45, "aryl chloride" }, { 46, "aryl bromide" }, { 47, "aryl iodide" }, { 48, "hemiacetal" }, { 49, "hydrazine" }, { 50, "hydrazone" }, { 51, "hydroxylamine" }, { 52, "imide" }, { 53, "imine" }, { 54, "iminium" }, { 55, "isocyanate" }, { 56, "isocyanide" }, { 57, "isonitrile" }, { 58, "isothiocyanate" }, { 59, "ketal" }, { 60, "ketene" }, { 61, "ketone" }, { 62, "ketoxime" }, { 63, "N oxide" }, { 64, "P oxide" }, { 65, "aryl nitrile" }, { 66, "alkyl nitrile" }, { 67, "aryl nitro" }, { 68, "alkyl nitro" }, { 69, "nitroso" }, { 70, "oxime" }, { 71, "peroxide" }, { 72, "aryl alcohol" }, { 73, "phenoxide" }, { 74, "phenoxy" }, { 76, "phosphine" }, { 77, "primary amide" }, { 78, "secondary amide" }, { 79, "tertiary amide" }, { 80, "primary alcohol" }, { 81, "secondary alcohol" }, { 82, "tertiary alcohol" }, { 83, "primary amine" }, { 84, "secondary amine" }, { 85, "tertiary amine" }, { 86, "sulfide" }, { 87, "sulfonamide" }, { 88, "sulfone" }, { 89, "sulfonic acid" }, { 90, "sulfoxide" }, { 91, "thioamide" }, { 92, "thiocarboxide" }, { 93, "thiocarboxylate" }, { 94, "thioester" }, { 95, "thiol" }, { 96, "thiourea" }, { 97, "urea" }, { 98, "vinyl" }, { 99, "aliphatic alcohol" }, { 100, "boronic acid" }, { 101, "phosphonium" }, { 102, "aryl boronic acid" } };
        public int count;
        public double Score;
    }
    public class PairBond
    {
        public List<int> maps = new List<int>(2);
        public string bondstr;

        public PairBond(int map1, int map2)
        {
            if (map2 >= map1)
            {
                maps.Add(map1);
                maps.Add(map2);
            }
            else
            {
                maps.Add(map2);
                maps.Add(map1);
            }
            bondstr = maps[0] + "-" + maps[1];
        }
    }

    public class ReturnList
    {
        private string conditionSeparator = "; ";
        private string referenceSeparator = ",";
        //public bool hasSimilarReaction;
        public bool HasCAS;
        //public bool IsDeprotection;
        public int HasRegioError;
        //public string QuerySmarts;
        public int TagID;
        public bool ToReserve;

        public List<int> RetroReactionIDs;

        public Retro Retro;

        public float yield { set; get; }

        public int reactionID { set; get; }

        public int citationID { set; get; }

        /// <summary>
        /// 步骤
        /// </summary>
        public string procedure { set; get; }

        /// <summary>
        /// 参考
        /// </summary>
        public string reference { set; get; }

        public string rxnsmiles
        {
            set
            {
                rxnsmilesInCode = value;
            }
            get
            {
                return rxnsmilesInCode;
            }
        }
        public string rxnsmilesInCode;

        public int isMore { set; get; }

        public int RetroReactionID { set; get; }
        public int Steps { set; get; }

        public List<Reaction> reactionObject { set; get; }

        public int RxnScore { set; get; }

        public string Reagent { set; get; }

        public string Catalyst { set; get; }

        public string Solvent { set; get; }

        public int CanMake { set; get; }

        public bool HasPrice { set; get; }

        public decimal TotalPrice { set; get; }

        public string condition { set; get; }

        public int ConditionCount { set; get; }

        public int Count { set; get; }
        public int TotalCount { set; get; }



        public string XianShiReference
        {
            get
            {
                return TrimReference(reference);
            }
        }

        public string XianShiCondition
        {
            get
            {
                if (condition == null)
                    return "";
                if (condition.EndsWith("|"))
                {
                    condition = (condition.Substring(0, condition.Length - 1));
                }
                return condition = condition.Replace("|", conditionSeparator);
            }
        }
        private string _productSmiles = string.Empty;
        public int EvaluationScore;
        //    public int[] excludedIds;
        public bool IsPerfect;
        public IEnumerable<string> noPriceReactifs;
        public bool isRedundent;
        public bool hasMoleculeID;
        public float similRank;
        public int similCdid;
        public string similSmiles;
        public float similReact;
        public double reactSimil;

        //public bool SemiCanMake;

        public string ProductSmiles
        {
            set { _productSmiles = value; }
            get
            {
                return _productSmiles;
            }
        }

        private string TrimReference(string refstring)
        {
            if (string.IsNullOrEmpty(refstring))
                return "";
            if (refstring.Contains("Gmelin"))
                refstring.Replace("Gmelin", "");
            string[] elements = refstring.Split(';');
            if (elements.Length < 2) return refstring;
            StringBuilder sb = new StringBuilder();
            bool beginAppend = false;
            for (int i = 1; i < elements.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(elements[i]))
                    continue;
                if (!beginAppend && Regex.IsMatch(elements[i], "\\d"))
                {
                    sb.Append(elements[i - 1] + referenceSeparator);
                    beginAppend = true;
                }
                if (beginAppend)
                    sb.Append(elements[i] + referenceSeparator);
            }
            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        public ReturnList Clone()
        {
            ReturnList entity = new ReturnList();
            entity.procedure = procedure;
            entity.reference = reference;
            entity.rxnsmilesInCode = rxnsmilesInCode;
            entity.yield = yield;
            entity.condition = condition;
            entity.reactionID = reactionID;
            entity.citationID = citationID;
            entity.RetroReactionID = RetroReactionID;
            entity.Retro = Retro;
            entity.Reagent = Reagent;
            entity.Catalyst = Catalyst;
            entity.Solvent = Solvent;
            entity.Steps = Steps;
            return entity;
        }
    }
}
