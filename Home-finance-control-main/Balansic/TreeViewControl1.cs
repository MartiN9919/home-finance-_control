using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Balansic.DB.Entities;

namespace Balansic.Pages.Controls
{
    public static class TreeViewControl
    {
        public static TreeView LastTree { get; set; }

        public static void SetUpTree(TreeView tree)
        {
            if (tree == null)
            {
                throw new ArgumentNullException("tree");
            }
            LastTree = tree;

            tree.CheckBoxes = true;
            tree.FullRowSelect = false;
            tree.ShowRootLines = true;
            tree.BeforeSelect += (o, e) =>
            {
                if (e.Node.Nodes.Count > 0)
                    e.Node.Toggle();
                else
                    e.Node.Checked = !e.Node.Checked;
                e.Cancel = true;
            };
            tree.BeforeCheck += (o, e) =>
            {
                if (e.Node.Nodes.Count > 0)
                {
                    bool hasUnchecked = false;
                    foreach (TreeNode child in e.Node.Nodes)
                        if (!child.Checked) { hasUnchecked = true; break; }
                    foreach (TreeNode child in e.Node.Nodes)
                    {
                        child.Checked = hasUnchecked;
                    }
                    if (hasUnchecked)
                        e.Node.Expand();
                    e.Cancel = true;
                }
            };
        }

        public static void FillSpendFilters()
        {
            FillSpendFilters(LastTree);
        }

        public static void FillSpendFilters(TreeView tree)
        {
            tree.Nodes.Clear();
            foreach (var pair in DB.DBManager.SpendFilters)
            {
                var root = AddRootNode(tree, pair.Key);
                AddChildNodes(root, pair.Value);
            }
        }

        public static FilterTreeNode AddRootNode(TreeView tree, Filter rootItem)
        {
            FilterTreeNode root = new FilterTreeNode();
            root.Item = rootItem;
            tree.Nodes.Add(root);
            return root;
        }

        public static void AddChildNodes(TreeNode parent, List<SpendFilter> filters)
        {
            foreach (var filter in filters)
            {
                FilterTreeNode node = new FilterTreeNode();
                node.Item = filter;
                parent.Nodes.Add(node);
            }
        }

        public static List<SpendFilter> GetSelection()
        {
            return GetSelection(LastTree);
        }

        public static List<SpendFilter> GetSelection(TreeView tree)
        {
            List<SpendFilter> ret = new List<SpendFilter>();

            if (tree.Nodes.Count > 0)
                FindSelected(tree.Nodes[0], ret);

            return ret;
        }

        private static void FindSelected(TreeNode node, List<SpendFilter> filters)
        {
            var root = node as FilterTreeNode;
            if (root == null) return;
            if (filters == null) 
                filters = new List<SpendFilter>();
            if (root.Nodes.Count > 0) 
                FindSelected(root.Nodes[0], filters);
            if (root.Checked)
            {
                var item = root.Item as SpendFilter;
                if (item != null)
                    filters.Add(item);
            }
            FindSelected(root.NextNode, filters);
        }
    }

    public class FilterTreeNode : TreeNode
    {
        private Filter _item;
        public Filter Item 
        {
            get { return _item; } 
            set
            {
                if (value != null)
                {
                    _item = value;
                    this.Text = _item.Name;
                }
            }
        }
        public long ItemIndex { get { return _item != null && _item.Id.HasValue ? _item.Id.Value : -1; } }

        public FilterTreeNode()
        {
            this.Text = string.Empty;
        }
    }
}
