import java.util.ArrayList;

abstract class HashTable {
    protected int table_size;
    protected int probe_count;
    protected int search_count;
    protected int count;
    
    public HashTable(int table_size) {
        this.table_size = table_size;
        probe_count = search_count = 0;
        count = 0;
    }
    public abstract void insert(int value);
    public abstract HashTableIndex search(int value);
    public abstract void delete(HashTableIndex index) throws HashTableOverflowError;
    public int getCount() {
        return count;
    }
    public double getProbesPerSearch() {
        if (search_count == 0) {
            return 0;
        }
        return ((double) probe_count) / ((double) search_count);
    }
    public int getProbeCount() {
        return probe_count;
    }
    public void setProbeCount(int value) {
        probe_count = value;
    }
    public int getSearchCount() {
        return search_count;
    }
    public void setSearchCount(int value) {
        search_count = value;
    }
    protected int hash(int value) {
        // multiplication method with A as suggested by Knuth
        double multiplier = (value * (Math.sqrt(5) - 1) / 2);
        double iPart = (long) multiplier;
        return (int) (Math.floor(table_size) * (multiplier - iPart));
    }
    
    static int randint(int min, int max) {
        // gives number in [min,max)
        return (int) (Math.random() * (max - min - 1) + min);
    }
    
    static int[] make_keys(int count, int min, int max) {
        if (count < 0) return null;
        int[] keys = new int[count];
        for (int i = 0; i < count; ++i) {
            keys[i] = randint(min, max);
        }
        return keys;
    }
    
    static int[] random_keys(int[] keys, int count) {
        int[] randkeys = new int[count];
        ArrayList<Integer> usedkeys = new ArrayList<Integer>(count);
        int i = 0;
        while (i < count) {
            int rand = randint(0, keys.length);
            Integer key = new Integer(rand);
            if (!usedkeys.contains(key)) {
                usedkeys.add(key);
                randkeys[i] = keys[rand];
                i++;
            }
        }
        return randkeys;
    }
    
    final static boolean QUIET = false;
    final static int TABLE_SIZE = 997;
    final static int KEYS = 1000;
    final static int MIN_KEY = 1;
    final static int MAX_KEY = 10000;
    final static int FIRST_INSERT = 900;
    
    public static void main(String[] args) {
        int table_size = TABLE_SIZE;
        int[] keys = make_keys(KEYS, MIN_KEY, MAX_KEY);
        ChainedHashTable c = new ChainedHashTable(table_size);
        LinearHashTable l = new LinearHashTable(table_size);
        QuadraticHashTable q = new QuadraticHashTable(table_size);
        DoubleHashTable d = new DoubleHashTable(table_size);
        // insert first 900 keys
        for (int i = 0, key = keys[i]; i < FIRST_INSERT; ++i, key = keys[i]) {
            c.insert(key);
            l.insert(key);
            q.insert(key);
            d.insert(key);
        }
        if (!QUIET) {
            System.out.println("Pre-deletion:");
            System.out.println("Chaining: " + c.toString());
            System.out.println("Linear: " + l.toString());
            System.out.println("Quadratic: " + q.toString());
            System.out.println("Double: " + d.toString());
        }
        System.out.println("Searching for 10 random keys and deleting them:");
        int[] inserted_keys = new int[FIRST_INSERT];
        for (int i = 0; i < FIRST_INSERT; ++i) inserted_keys[i] = keys[i];
        int[] to_delete = random_keys(inserted_keys, 10);
        for (int delete : to_delete) {
            HashTableIndex loc;
            int probes;
            // chaining
            probes = c.getProbeCount();
            loc = c.search(delete);
            if (loc != null) {
                System.out.format("Chaining: %d found at slot %d, offset %d, " +
                        "%d probes\n", delete, loc.getIndex(), loc.getOffset(),
                        c.getProbeCount() - probes);
                c.delete(loc);
            } else {
                System.out.format("Chaining: %d not found!\n", delete);
            }
            // linear
            probes = l.getProbeCount();
            loc = l.search(delete);
            if (loc != null) {
                System.out.format("Linear: %d found at slot %d, %d probes\n",
                        delete, loc.getIndex(), l.getProbeCount() - probes);
                l.delete(loc);
            } else {
                System.out.format("Linear: %d not found!\n", delete);
            }
            // quadratic
            probes = q.getProbeCount();
            loc = q.search(delete);
            if (loc != null) {
                System.out.format("Quadratic: %d found at slot %d, %d probes\n",
                        delete, loc.getIndex(), q.getProbeCount() - probes);
                q.delete(loc);
            } else {
                System.out.format("Quadtratic: %d not found!\n", delete);
            }
            // double
            probes = d.getProbeCount();
            loc = d.search(delete);
            if (loc != null) {
                System.out.format("Double: %d found at slot %d, %d probes\n",
                        delete, loc.getIndex(), d.getProbeCount() - probes);
                d.delete(loc);
            } else {
                System.out.format("Double: %d not found!\n", delete);
            }
        }
        if (!QUIET) {
            // print post deletion stuff
            System.out.println("Post-deletion:");
            System.out.println("Chaining: " + c.toString());
            System.out.println("Linear: " + l.toString());
            System.out.println("Quadratic: " + q.toString());
            System.out.println("Double: " + d.toString());
        }
        
        // insert last 100 keys
        for (int i = FIRST_INSERT; i < keys.length; ++i) {
            c.insert(keys[i]);
            l.insert(keys[i]);
            q.insert(keys[i]);
            d.insert(keys[i]);
        }
        
        // reset the probe and search counts since we only want avgs for
        // last step
        c.setProbeCount(0); c.setSearchCount(0);
        l.setProbeCount(0); l.setSearchCount(0);
        q.setProbeCount(0); q.setSearchCount(0);
        d.setProbeCount(0); d.setSearchCount(0);
        
        // search for 10 random keys, probe and search counts are recorded
        // in the hash table objects
        System.out.println("Searching for 10 random keys:");
        int[] to_search = random_keys(keys, 10);
        for (int search : to_search) {
            HashTableIndex loc;
            int probes;
            // chaining
            probes = c.getProbeCount();
            loc = c.search(search);
            if (loc != null) {
                System.out.format("Chaining: %d found at slot %d, offset %d, " +
                        "%d probes\n", search, loc.getIndex(), loc.getOffset(),
                        c.getProbeCount() - probes);
            } else {
                System.out.format("Chaining: %d not found!\n", search);
            }
            // linear
            probes = l.getProbeCount();
            loc = l.search(search);
            if (loc != null) {
                System.out.format("Linear: %d found at slot %d, %d probes\n",
                        search, loc.getIndex(), l.getProbeCount() - probes);
            } else {
                System.out.format("Linear: %d not found!\n", search);
            }
            // quadratic
            probes = q.getProbeCount();
            loc = q.search(search);
            if (loc != null) {
                System.out.format("Quadratic: %d found at slot %d, %d probes\n",
                        search, loc.getIndex(), q.getProbeCount() - probes);
            } else {
                System.out.format("Quadtratic: %d not found!\n", search);
            }
            // double
            probes = d.getProbeCount();
            loc = d.search(search);
            if (loc != null) {
                System.out.format("Double: %d found at slot %d, %d probes\n",
                        search, loc.getIndex(), d.getProbeCount() - probes);
            } else {
                System.out.format("Double: %d not found!\n", search);
            }
        }
        // print out n, m, n/m (alpha, load factor) and avg probes per search
        System.out.format(
"+-----------------+---------------------------------------------------+\n" +
"|                 |         Search performance                        |\n" +
"+-----------------+------------+------------+------------+------------+\n" +
"| Algorithm       |     n      |     m      |    alpha   |#probes(avg)|\n" +
"+-----------------+------------+------------+------------+------------+\n" +
"| Chaining        |    % 4d    |   % 4d     |   % .4f  |   % 4d     |\n" +
"+-----------------+------------+------------+------------+------------+\n" +
"| Linear probe    |    % 4d    |   % 4d     |   % .4f  |   % 4d     |\n" +
"+-----------------+------------+------------+------------+------------+\n" +
"| Quadratic probe |    % 4d    |   % 4d     |   % .4f  |   % 4d     |\n" +
"+-----------------+------------+------------+------------+------------+\n" +
"| Double hashing  |    % 4d    |   % 4d     |   % .4f  |   % 4d     |\n" +
"+-----------------+------------+------------+------------+------------+\n",
            c.getCount(), table_size, ((double) c.getCount()) / table_size,
            (int) (((double) c.getProbeCount()) / c.getSearchCount()),
            l.getCount(), table_size, ((double) l.getCount()) / table_size,
            (int) (((double) l.getProbeCount()) / l.getSearchCount()),
            q.getCount(), table_size, ((double) q.getCount()) / table_size,
            (int) (((double) q.getProbeCount()) / q.getSearchCount()),
            d.getCount(), table_size, ((double) d.getCount()) / table_size,
            (int) (((double) d.getProbeCount()) / d.getSearchCount())
        );
    }
}
