import java.util.ArrayList;
import java.lang.StringBuilder;
import java.util.LinkedList;

public class ChainedHashTable extends HashTable {
    private ArrayList<LinkedList<HashTableItem>> table;
    
    public ChainedHashTable(int table_size) {
        super(table_size);
        this.table = new ArrayList<LinkedList<HashTableItem>>(table_size);
        for (int i = 0; i < this.table_size; ++i) {
            table.add(i, null);
        }
    }
    
    public String toString() {
        StringBuilder sb = new StringBuilder();
        int index = 0;
        for (LinkedList<HashTableItem> list : table) {
            if (list == null) {
                index++;
                continue;
            }
            StringBuilder chain = new StringBuilder("[");
            for (HashTableItem item : list) {
                chain.append("" + item.getValue() + ", ");
            }
            chain.append("]");
            sb.append(String.format("%d: %s, ", index, chain.toString()));
            index++;
        }
        return sb.toString();
    }

    @Override
    public void insert(int value) {
        int index = hash(value);
        if (table.get(index) == null) {
            table.set(index, new LinkedList<HashTableItem>());
        }
        table.get(index).add(new HashTableItem(value));
        count++;
    }

    @Override
    public HashTableIndex search(int value) {
        search_count++;
        probe_count++;
        int index = hash(value);
        if (table.get(index) == null) {
            return null;
        }
        for (int i = 0; i < table.get(index).size(); ++i) {
            probe_count++;
            if (table.get(index).get(i).getValue() == value) {
                return new HashTableIndex(index, i);
            }
        }
        return null;
    }

    @Override
    public void delete(HashTableIndex index) throws HashTableOverflowError {
        table.get(index.getIndex()).remove(index.getOffset());
        count--;
    }
}
