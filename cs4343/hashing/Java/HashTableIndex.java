
public class HashTableIndex {
    private int index;
    private int offset;
    
    public HashTableIndex(int index, int offset) {
        this.index = index;
        this.offset = offset;
    }
    
    public HashTableIndex(int index) {
        this(index, -1);
    }
    
    public String toString() {
        if (offset < 0) {
            return String.format("%d", index);
        } else {
            return String.format("%d offset %d", index, offset);
        }
    }
    
    public int getIndex() {
        return index;
    }
    
    public int getOffset() {
        return offset;
    }
}
