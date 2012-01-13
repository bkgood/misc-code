import java.util.ArrayList;

class LinearHashTable extends HashTable {
    private ArrayList<HashTableItem> table;

    public LinearHashTable(int table_size) {
        super(table_size);
        this.table = new ArrayList<HashTableItem>(table_size);
        for (int i = 0; i < this.table_size; ++i) {
            table.add(i, null);
        }
    }
    public String toString() {
        StringBuilder sb = new StringBuilder();
        int index = 0;
        for (HashTableItem item : table) {
            if (item == null) {
                sb.append(String.format("%d: null, ", index));
            } else if (item.isDeleted()) {
                sb.append(String.format("%d: deleted, ", index));
            } else {
                sb.append(String.format("%d: %d, ", index, item.getValue()));
            }
            index++;
        }
        return sb.toString();
    }
    
    @Override
    public void insert(int value) {
        int index = hash(value);
        int starting_index = index;
        HashTableItem item = table.get(index);
        while (item != null && !item.isDeleted()) {
            index = (index + 1) % table_size;
            if (index == starting_index) {
                throw new HashTableOverflowError();
            }
            item = table.get(index);
        }
        table.set(index, new HashTableItem(value));
        count++;
    }

    @Override
    public HashTableIndex search(int value) {
        search_count++;
        probe_count++;
        int index = hash(value);
        int starting_index = index;
        HashTableItem item = table.get(index);
        while (item != null && item.getValue() != value) {
            probe_count++;
            index = (index + 1) % table_size;
            if (index == starting_index) {
                return null;
            }
            item = table.get(index);
        }
        if (item == null || item.isDeleted()) {
            return null;
        }
        return new HashTableIndex(index);
    }

    @Override
    public void delete(HashTableIndex index) {
        table.get(index.getIndex()).setDeleted(true);
        count--;
    }
}