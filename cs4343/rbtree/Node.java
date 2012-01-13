// William Good
// CS4343
// Due Nov 30, 2010
// Programming assignment 3 (red-black trees)

class Node {
    private Node parent;
    private Node left;
    private Node right;
    private int value;
    private boolean red;
    
    public Node(int value, boolean red) {
        setValue(value);
        setRed(red);
    }
    
    public Node(int value) {
        this(value, true);
    }

    public void setParent(Node parent) {
        this.parent = parent;
    }

    public Node getParent() {
        return parent;
    }

    public void setLeft(Node left) {
        this.left = left;
    }

    public Node getLeft() {
        return left;
    }

    public void setRight(Node right) {
        this.right = right;
    }

    public Node getRight() {
        return right;
    }

    public void setValue(int value) {
        this.value = value;
    }

    public int getValue() {
        return value;
    }

    public void setRed(boolean red) {
        this.red = red;
    }

    public boolean isRed() {
        return red;
    }
    
    public String toString() {
        return String.format("%d%s", value, red ? "R" : "B");
    }
    
    public String toString(boolean verbose) {
        if (!verbose) {
            return toString();
        } else {
            return String.format("Node %s: parent %s left %s right %s\n",
                    toString(), parent.toString(), left.toString(),
                    right.toString());
        }
    }
}

class NilNode extends Node {
    public NilNode() {
        super(0, false);
        setParent(this);
        setLeft(this);
        setRight(this);
    }
    
    @Override
    public void setValue(int value) {
    }
    
    @Override
    public void setRed(boolean red) {
    }
    
    @Override
    public String toString() {
        return "nil";
    }
}
