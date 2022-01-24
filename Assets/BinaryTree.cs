using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BinaryTree
{
    public string data;
    public BinaryTree left;
    public BinaryTree right;
    public BinaryTree parent;

    public BinaryTree()
    {

    }
    public BinaryTree(string ch)
    {
        data = ch;
    }

    public static void Build(BinaryTree node, Queue<string> queue, int depth, int target)
    {
        if (queue.Count == 0)
        {
            return;
        }

        if (depth < target)
        {
            node.left = new BinaryTree() { parent = node };
            Build(node.left, queue, depth + 1, target);
            node.right = new BinaryTree() { parent = node };
            Build(node.right, queue, depth + 1, target);
        }
        else
        {
            node.left = new BinaryTree(queue.Dequeue());
            if(queue.Count>0) node.right = new BinaryTree(queue.Dequeue());
            return;
        }
    }

    public static void PrintTree(BinaryTree node)
    {
        if (node is null) return;
        if (node.left is null)
        {
            Console.Write(node.data + " ");
        }
        else
        {
            PrintTree(node.left);
            PrintTree(node.right);
        }
    }

    public static void GetTree(BinaryTree node, List<string> list)
    {
        if (node is null) return;
        if (node.left is null)
        {
            list.Add(node.data);
        }
        else
        {
            GetTree(node.left, list);
            GetTree(node.right, list);
        }

    }

}


