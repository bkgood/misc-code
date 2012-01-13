
class HashTableItem {
    private int value;
    private boolean deleted;
    
    public HashTableItem(int value) {
        setValue(value);
        setDeleted(false);
    }

    public int getValue() {
        return value;
    }
    
    public void setValue(int value) {
        this.value = value;
    }

    public boolean isDeleted() {
        return deleted;
    }

    public void setDeleted(boolean deleted) {
        this.deleted = deleted;
    }
}
