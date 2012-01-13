// William Good
// CS4343
// Due Nov 30, 2010
// Programming assignment 3 (red-black trees)

import java.util.ArrayList;

public class Tree {
    private Node root;
    private Node nil;
    
    enum TraversalOrder {
        PREORDER, INORDER, POSTORDER,
    }
    
    public Tree() {
        nil = new NilNode();
        root = nil;
    }
    
    public void insert(Node node) {
        Node parent = nil;
        Node pos = root;
        while (pos != nil) {
            parent = pos;
            if (node.getValue() < pos.getValue()) {
                pos = pos.getLeft();
            } else {
                pos = pos.getRight();
            }
        }
        node.setParent(parent);
        if (parent == nil) {
            root = node;
        } else {
            if (node.getValue() < parent.getValue()) {
                parent.setLeft(node);
            } else {
                parent.setRight(node);
            }
        }
        node.setLeft(nil);
        node.setRight(nil);
        node.setRed(true);
        insertFixup(node);
    }
    
    public void delete(Node node) {
        Node splice;
        Node child;
        if (node.getLeft() == nil || node.getRight() == nil) {
            splice = node;
        } else {
            splice = successor(node);
        }
        if (splice.getLeft() != nil) {
            child = splice.getLeft();
        } else {
            child = splice.getRight();
        }
        child.setParent(splice.getParent());
        if (splice.getParent() == nil) {
            root = child;
        } else if (splice == splice.getParent().getLeft()) {
            splice.getParent().setLeft(child);
        } else {
            splice.getParent().setRight(child);
        }
        if (splice != node) {
            splice.setParent(node.getParent());
            splice.setLeft(node.getLeft());
            splice.setRight(node.getRight());
            node.getLeft().setParent(splice);
            node.getRight().setParent(splice);
            if (node == root) {
                root = splice;
            } else if (node == node.getParent().getLeft()) {
                node.getParent().setLeft(splice);
            } else {
                node.getParent().setRight(splice);
            }
            splice.setRed(node.isRed());
        }
        if (!splice.isRed()) {
            deleteFixup(child);
        }
    }
    
    public void print(TraversalOrder order, boolean verbose) {
        switch (order) {
        case PREORDER:
            printPreorder(root);
            System.out.println();
            break;
        case INORDER:
            printInorder(root, verbose);
            if (!verbose) {
                System.out.println();
            }
            break;
        case POSTORDER:
            printPostorder(root);
            System.out.println();
            break;
        }
    }
    
    public void print(TraversalOrder order) {
        print(order, false);
    }
    
    private void insertFixup(Node node) {
        while (node.getParent().isRed()) {
            if (node.getParent() == node.getParent().getParent().getLeft()) {
                Node uncle = node.getParent().getParent().getRight();
                if (uncle.isRed()) {
                    if (DEBUG) System.out.println("Case 1 (Left)");
                    node.getParent().setRed(false);
                    uncle.setRed(false);
                    node.getParent().getParent().setRed(true);
                    node = node.getParent().getParent();
                } else {
                    if (node == node.getParent().getRight()) {
                        if (DEBUG) System.out.println("Case 2 (Left)");
                        node = node.getParent();
                        leftRotate(node);
                    }
                    if (DEBUG) System.out.println("Case 3 (Left)");
                    node.getParent().setRed(false);
                    node.getParent().getParent().setRed(true);
                    rightRotate(node.getParent().getParent());
                }
            } else {
                Node uncle = node.getParent().getParent().getLeft();
                if (uncle.isRed()) {
                    if (DEBUG) System.out.println("Case 1 (Right)");
                    node.getParent().setRed(false);
                    uncle.setRed(false);
                    node.getParent().getParent().setRed(true);
                    node = node.getParent().getParent();
                } else {
                    if (node == node.getParent().getLeft()) {
                        if (DEBUG) System.out.println("Case 2 (Right)");
                        node = node.getParent();
                        rightRotate(node);
                    }
                    if (DEBUG) System.out.println("Case 3 (Right)");
                    node.getParent().setRed(false);
                    node.getParent().getParent().setRed(true);
                    leftRotate(node.getParent().getParent());
                }
            }
        }
        root.setRed(false);
    }
    
    private void deleteFixup(Node node) {
        while (node != root && !node.isRed()) {
            if (node == node.getParent().getLeft()) {
                Node sibling = node.getParent().getRight();
                if (sibling.isRed()) {
                    if (DEBUG) System.out.println("Case 1 (Left)");
                    sibling.setRed(false);
                    node.getParent().setRed(true);
                    leftRotate(node.getParent());
                    sibling = node.getParent().getRight();
                }
                if (!sibling.getLeft().isRed() && !sibling.getRight().isRed()) {
                    if (DEBUG) System.out.println("Case 2 (Left)");
                    sibling.setRed(true);
                    node = node.getParent();
                } else {
                    if (!sibling.getRight().isRed()) {
                        if (DEBUG) System.out.println("Case 3 (Left)");
                        sibling.getLeft().setRed(false);
                        sibling.setRed(true);
                        // have to save the parent of node, if node is nil
                        // then rotate overwrites parent of nil incl our
                        // nil (since it's just a bunch of refs to a single
                        // nil node)
                        Node saved_parent = node.getParent();
                        rightRotate(sibling);
                        node.setParent(saved_parent);
                        sibling = node.getParent().getRight();
                    }
                    if (DEBUG) System.out.println("Case 4 (Left)");
                    sibling.setRed(node.getParent().isRed());
                    node.getParent().setRed(false);
                    sibling.getRight().setRed(false);
                    leftRotate(node.getParent());
                    node = root;
                }
            } else {
                Node sibling = node.getParent().getLeft();
                if (sibling.isRed()) {
                    if (DEBUG) System.out.println("Case 1 (Right)");
                    sibling.setRed(false);
                    node.getParent().setRed(true);
                    rightRotate(node.getParent());
                    sibling = node.getParent().getLeft();
                }
                if (!sibling.getLeft().isRed() && !sibling.getRight().isRed()) {
                    if (DEBUG) System.out.println("Case 2 (Right)");
                    sibling.setRed(true);
                    node = node.getParent();
                } else {
                    if (!sibling.getLeft().isRed()) {
                        if (DEBUG) System.out.println("Case 3 (Right)");
                        sibling.getRight().setRed(false);
                        sibling.setRed(true);
                        // have to save the parent of node, if node is nil
                        // then rotate overwrites parent of nil incl our
                        // nil (since it's just a bunch of refs to a single
                        // nil node)
                        Node saved_parent = node.getParent();
                        leftRotate(sibling);
                        node.setParent(saved_parent);
                        sibling = node.getParent().getLeft();
                    }
                    if (DEBUG) System.out.println("Case 4 (Right)");
                    sibling.setRed(node.getParent().isRed());
                    node.getParent().setRed(false);
                    sibling.getLeft().setRed(false);
                    rightRotate(node.getParent());
                    node = root;
                }
            }
        }
        node.setRed(false);
    }
    
    private void leftRotate(Node node) {
        Node pivot = node.getRight();
        node.setRight(pivot.getLeft());
        pivot.getLeft().setParent(node);
        pivot.setParent(node.getParent());
        if (node.getParent() == nil) {
            root = pivot;
        } else if (node == node.getParent().getLeft()) {
            node.getParent().setLeft(pivot);
        } else { 
            node.getParent().setRight(pivot);
        }
        pivot.setLeft(node);
        node.setParent(pivot);
    }
    
    private void rightRotate(Node node) {
        Node pivot = node.getLeft();
        node.setLeft(pivot.getRight());
        pivot.getRight().setParent(node);
        pivot.setParent(node.getParent());
        if (node.getParent() == nil) {
            root = pivot;
        } else if (node == node.getParent().getLeft()) {
            node.getParent().setLeft(pivot);
        } else { 
            node.getParent().setRight(pivot);
        }
        pivot.setRight(node);
        node.setParent(pivot);
    }
    
    private Node successor(Node node) {
        if (node.getRight() != nil) {
            while (node.getLeft() != nil) {
                node = node.getLeft();
            }
            return node;
        }
        Node parent = node.getParent();
        while (parent != nil && node == parent.getRight()) {
            node = parent;
            parent = parent.getParent();
        }
        return parent;
    }
    
    private void printPreorder(Node node) {
        System.out.print(node.toString() + " ");
        if (node.getLeft() != nil) printPreorder(node.getLeft());
        if (node.getRight() != nil) printPreorder(node.getRight());
    }
    
    private void printInorder(Node node, boolean verbose) {
        if (!verbose) System.out.print("(");
        if (node.getLeft() != nil) {
            printInorder(node.getLeft(), verbose);
            if (!verbose) System.out.print(", ");
        }
        System.out.print(node.toString(verbose));
        if (node.getRight() != nil) {
            if (!verbose) System.out.print(", ");
            printInorder(node.getRight(), verbose);
        }
        if (!verbose) System.out.print(")");
    }
    
    private void printPostorder(Node node) {
        if (node.getLeft() != nil) printPostorder(node.getLeft());
        if (node.getRight() != nil) printPostorder(node.getRight());
        System.out.print(node.toString() + " ");
    }
    
    final static boolean DEBUG = false;
    final static int NODES = 40;
    final static int MIN_KEY = 0;
    final static int MAX_KEY = 50;
    final static int INSERT_FIRST = 30;
    final static int DELETE = 5;
    
    static int rand(int min, int max) {
        // gives number in [min,max)
        return (int) (Math.random() * (max - min) + min);
    }
    
    static Node[] makeNodes(int count, int min, int max) {
        if (count < 0) count = 0;
        Node[] keys = new Node[count];
        ArrayList<Integer> usedKeys = new ArrayList<Integer>(count);
        int i = 0;
        while (i < count) {
            Integer key = new Integer(rand(min, max));
            if (usedKeys.contains(key)) continue;
            usedKeys.add(key);
            keys[i++] = new Node(key.intValue()); 
        }
        return keys;
    }
    
    static Node[] randomKeys(Node[] nodes, int max, int count) {
        if (count < 0) count = 0;
        Node[] keys = new Node[count];
        ArrayList<Node> usedNodes = new ArrayList<Node>(count);
        int i = 0;
        while (i < count) {
            Node key = nodes[rand(0, max)];
            if (usedNodes.contains(key)) continue;
            usedNodes.add(key);
            keys[i++] = key; 
        }
        return keys;
    }
    
    public static void main(String[] args) {
        Node[] nodes = makeNodes(NODES, MIN_KEY, MAX_KEY);
        Tree tree = new Tree();
        //1) insert the first 30 keys
        for (int i = 0; i < INSERT_FIRST; ++i) {
            tree.insert(nodes[i]);
        }
        // 2) print the tree (keys and corresponding colors) using inorder,
        // preorder and postorder tree-walks. In the case of inorder tree-walk,
        // use parenthesis to properly identify the subtrees by enclosing the
        // parent and children separated by commas within the parenthesis.
        System.out.println("After insert of " + INSERT_FIRST + " keys\n" +
                "Preorder:");
        tree.print(TraversalOrder.PREORDER);
        System.out.println("Inorder:");
        tree.print(TraversalOrder.INORDER);
        System.out.println("Postorder:");
        tree.print(TraversalOrder.POSTORDER);
        // 3) delete from the tree five keys selected at random
        for (Node toDelete : randomKeys(nodes, INSERT_FIRST, DELETE)) {
            System.out.println("Deleting " + toDelete.getValue());
            tree.delete(toDelete);
            // 4) print the (parenthesized) tree (keys and corresponding
            // colors) using inorder tree-walk after each deletion.
            tree.print(TraversalOrder.INORDER);
        }
        // 5) insert the remaining 10 keys.
        for (int i = INSERT_FIRST; i < NODES; ++i) {
            tree.insert(nodes[i]);
        }
        // 6) print the final tree (keys and corresponding colors) using inorder
        // tree-walk.
        System.out.println("After final insert: inorder:");
        tree.print(TraversalOrder.INORDER);
    }
}
