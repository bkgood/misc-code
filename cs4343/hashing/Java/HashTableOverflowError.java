
@SuppressWarnings("serial")
public class HashTableOverflowError extends Error {
    public HashTableOverflowError() {
        super("The hash table overflowed, increase table size.");
    }
}
