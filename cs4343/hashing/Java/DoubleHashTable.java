import java.util.ArrayList;


class DoubleHashTable extends HashTable {
    private ArrayList<HashTableItem> table;
    
    public DoubleHashTable(int table_size) {
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
        int second_hash = second_hash(value);
        HashTableItem item = table.get(index);
        while (item != null && !item.isDeleted()) {
            index = (index + second_hash) % table_size;
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
        int index = hash(value);
        int starting_index = index;
        int second_hash = second_hash(value);
        HashTableItem item = table.get(index);
        while (item != null && item.getValue() != value) {
            probe_count++;
            index = (index + second_hash) % table_size;
            if (index == starting_index) {
                return null;
            }
            item = table.get(index);
        }
        return item == null ? null : new HashTableIndex(index);
    }

    @Override
    public void delete(HashTableIndex index) throws HashTableOverflowError {
        table.get(index.getIndex()).setDeleted(true);
        count--;
    }

    private int second_hash(int value) {
        // second hash uses division. Subtracts 1 from the divisor so that
        // the remainder plus 1 will never be evenly divided by the table size
        // (incl. zero)
        return value % (table_size - 1) + 1;
    }
}
