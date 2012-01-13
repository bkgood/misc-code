#!/usr/bin/env python2
# William Good
# CS4343
# Due Nov 9, 2010
# Programming assignment 2

import math
from random import randint
from decimal import Decimal

DELETED = "DELETED"
QUADRATIC_CONSTANTS = (1, 1)

'''[Mostly] abstract base class for the hash tables'''
class HashTable(object):
    '''Initializes the hash table, using a given tablesize and hashing function.
    If no hashing method is given, the multiplication method from the text is
    used with A as suggested by Knuth.'''
    def __init__(self, table_size=1, hash=None):
        self.table_size = table_size
        if not hash:
            hash = lambda x: (
                # multiplication method with A as suggested by Knuth
                int(math.floor(table_size*math.modf(x*(math.sqrt(5)-1)/2)[0]))
            )
        self.hash = hash
        self.table = list()
        for x in xrange(table_size):
            self.table.append(None)
        self.probecount = Decimal(0) # number of probes used in searching
        self.searchcount = Decimal(0) # number of searches performed
        self.n = 0 # number of elements in table
    def __unicode__(self):
        raise NotImplementedError
    def __str__(self):
        return self.__unicode__()
    def insert(self, value):
        raise NotImplementedError
    def search(self, value):
        raise NotImplementedError
    def delete(self, value):
        raise NotImplementedError
    def probespersearch(self):
        if self.searchcount == 0:
            return Decimal(0)
        return self.probecount / self.searchcount

class HashTableOverflowError(Exception):
    def __init__(self):
        self.value = "The hash table overflowed, increase table size."
    def __str__(self):
        return self.value

class ChainedHashTable(HashTable):
    def insert(self, value):
        index = self.hash(value) 
        if not self.table[index]:
            self.table[index] = list()
        self.table[index].append(value)
        self.n += 1
    '''Searches for a value, returns the position when found as (index, offset)
    where index is the index of the list in which the value was located and
    offset is where the value lies in that list.'''
    def search(self, value):
        self.searchcount += 1
        self.probecount += 1
        index = self.hash(value)
        if not self.table[index]:
            return None
        for i in xrange(len(self.table[index])):
            self.probecount += 1
            if self.table[index][i] == value:
                return (index, i)
        return None
    def delete(self, index, offset):
        del self.table[index][offset]
        self.n -= 1

class LinearHashTable(HashTable):
    def insert(self, value):
        index = starting_index = self.hash(value)
        while self.table[index] not in (None, DELETED):
            index = (index + 1) % self.table_size
            if index == starting_index:
                raise HashTableOverflowError()
        self.table[index] = value
        self.n += 1
    def search(self, value):
        self.searchcount += 1
        self.probecount += 1
        index = starting_index = self.hash(value)
        while self.table[index] != value and self.table[index] is not None:
            self.probecount += 1
            index = (index + 1) % self.table_size
            if index == starting_index:
                return None
        if self.table[index] in (None, DELETED):
            return None
        return index
    def delete(self, index):
        self.table[index] = DELETED
        self.n -= 1

class QuadraticHashTable(HashTable):
    def insert(self, value):
        index = starting_index = self.hash(value)
        i = 1
        while self.table[index] not in (None, DELETED):
            index = (index + QUADRATIC_CONSTANTS[0] * i
                + QUADRATIC_CONSTANTS[1] * pow(i, 2)) % self.table_size
            i += 1
            if index == starting_index:
                raise HashTableOverflowError()
        self.table[index] = value
        self.n += 1
    def search(self, value):
        self.searchcount += 1
        index = starting_index = self.hash(value)
        i = 1
        self.probecount += 1
        while self.table[index] != value and self.table[index] is not None:
            self.probecount += 1
            index = (index + QUADRATIC_CONSTANTS[0] * i
                + QUADRATIC_CONSTANTS[1] * pow(i, 2)) % self.table_size
            i += 1
            if index == starting_index:
                return None
        if self.table[index] is None:
            return None
        return index
    def delete(self, index):
        self.table[index] = DELETED
        self.n -= 1

class DoubleHashTable(HashTable):
    def __init__(self, **kwargs):
        super(DoubleHashTable, self).__init__(**kwargs)
        # second hash used division. Subtracts 1 from the divisor so that
        # the remainder plus one will never be evenly divided by m (including
        # zero)
        self.second_hash = lambda x: x % (self.table_size - 1) + 1
    def insert(self, value):
        index = starting_index = self.hash(value)
        second_hash = self.second_hash(value)
        while self.table[index] not in (None, DELETED):
            index = (index + second_hash) % self.table_size
            if index == starting_index:
                raise HashTableOverflowError()
        self.table[index] = value
        self.n += 1
    def search(self, value):
        self.searchcount += 1
        index = starting_index = self.hash(value)
        second_hash = self.second_hash(value)
        self.probecount += 1
        while self.table[index] != value and self.table[index] is not None:
            self.probecount += 1
            index = (index + second_hash) % self.table_size
            if index == starting_index:
                return None
        if self.table[index] is None:
            return None
        return index
    def delete(self, index):
        self.table[index] = DELETED
        self.n -= 1

def main(quiet=False):
    # table_size (m) is 997 because the book recommends m be prime when the 
    # division-based hashing is used (division is used in the second hash in
    # the double hashing). The main hash used it multiplication-based, which
    # the books described as mostly m-agnostic
    table_size = 997
    # get a list of 1000 keys, between 1 and 10000 inclusive
    keys = [randint(1, 10000) for x in xrange(1000)]
    # create our hash tables, using a multiplicative hashing method by default
    c = ChainedHashTable(table_size=table_size)
    l = LinearHashTable(table_size=table_size)
    q = QuadraticHashTable(table_size=table_size)
    d = DoubleHashTable(table_size=table_size)
    # insert the first 900 keys
    for x in keys[0:900]:
        c.insert(x)
        l.insert(x)
        q.insert(x)
        d.insert(x)
    if not quiet:
        # print out all the tables... should be nasty
        print "Pre-deletion:"
        print "Chaining: " + ', '.join([
            "%d: %s" % (x, str(c.table[x])) for x in xrange(len(c.table))
        ])
        print "Linear: " + ', '.join([
            "%d: %s" % (x, l.table[x]) for x in xrange(len(l.table))
        ])
        print "Quadratic: " + ', '.join([
            "%d: %s" % (x, q.table[x]) for x in xrange(len(q.table))
        ])
        print "Double: " + ', '.join([
            "%d: %s" % (x, d.table[x]) for x in xrange(len(d.table))
        ])
    # delete 10 random keys after searching for them. note that it's possible 
    # for n to not be 890 after this if the same key is picked twice (happened
    # in testing) and the key only exists in the key list once
    for x in [keys[randint(0, 899)] for x in xrange(10)]:
        probes = c.probecount
        loc = c.search(x)
        # returning None signifies 'value not found'
        if loc is not None:
            print "Chaining: %d found at slot %d, offset %d, %d probes" % (
                x, loc[0], loc[1], c.probecount - probes)
            c.delete(loc[0], loc[1])
        else:
            print "Chaining: %d not found!" % (x,)
        probes = l.probecount
        loc = l.search(x)
        if loc is not None:
            print "Linear: %d found at slot %d, %d probes" % (x, loc,
                l.probecount - probes)
            l.delete(loc)
        else:
            print "Linear: %d not found!" % (x,)
        probes = q.probecount
        loc = q.search(x)
        if loc is not None:
            print "Quadratic: %d found at slot %d, %d probes" % (x, loc,
                q.probecount - probes)
            q.delete(loc)
        else:
            print "Quadratic: %d not found!" % (x,)
        probes = d.probecount
        loc = d.search(x)
        if loc is not None:
            print "Double: %d found at slot %d, %d probes" % (x, loc,
                d.probecount - probes)
            d.delete(loc)
        else:
            print "Double: %d not found!" % (x,)
    if not quiet:
        print "Post-deletion:"
        # print out all the tables... should be nasty again
        print "Chaining: " + ', '.join([
            "%d: %s" % (x, str(c.table[x])) for x in xrange(len(c.table))
        ])
        print "Linear: " + ', '.join([
            "%d: %s" % (x, l.table[x]) for x in xrange(len(l.table))
        ])
        print "Quadratic: " + ', '.join([
            "%d: %s" % (x, q.table[x]) for x in xrange(len(q.table))
        ])
        print "Double: " + ', '.join([
            "%d: %s" % (x, d.table[x]) for x in xrange(len(d.table))
        ])
    # insert the last 100 keys
    for x in keys[900:1000]:
        c.insert(x)
        l.insert(x)
        q.insert(x)
        d.insert(x)
    # reset the probe and search counts, since we only want the average probes
    # per search for the last step
    for x in (c, l, q, d):
        x.probecount = 0
        x.searchcount = 0
    # search for 10 random keys, probe and search counts are recorded in the
    # hash table objects
    for x in [keys[randint(0, 999)] for x in xrange(10)]:
        probes = c.probecount
        loc = c.search(x)
        if loc is not None:
            print "Chaining: %d found at slot %d, offset %d, %d probes" % (
                x, loc[0], loc[1], c.probecount - probes)
        else:
            print "Chaining: %d not found!" % (x,)
        probes = l.probecount
        loc = l.search(x)
        if loc is not None:
            print "Linear: %d found at slot %d, %d probes" % (x, loc,
                l.probecount - probes)
        else:
            print "%Linear: d not found!" % (x,)
        probes = q.probecount
        loc = q.search(x)
        if loc is not None:
            print "Quadratic: %d found at slot %d, %d probes" % (x, loc,
                q.probecount - probes)
        else:
            print "Quadratic: %d not found!" % (x,)
        probes = d.probecount
        loc = d.search(x)
        if loc is not None:
            print "Double: %d found at slot %d, %d probes" % (x, loc,
                d.probecount - probes)
        else:
            print "Double: %d not found!" % (x,)
    # print out n, m, n/m (alpha, load factor) and avg probes per search
    print ("""\
+-----------------+---------------------------------------------------+
|                 |         Search performance                        |
+-----------------+------------+------------+------------+------------+
| Algorithm       |     n      |     m      |    alpha   |#probes(avg)|
+-----------------+------------+------------+------------+------------+
| Chaining        |    % 4d    |   % 4d     |   % .4f  |   % 4d     |
+-----------------+------------+------------+------------+------------+
| Linear probe    |    % 4d    |   % 4d     |   % .4f  |   % 4d     |
+-----------------+------------+------------+------------+------------+
| Quadratic probe |    % 4d    |   % 4d     |   % .4f  |   % 4d     |
+-----------------+------------+------------+------------+------------+
| Double hashing  |    % 4d    |   % 4d     |   % .4f  |   % 4d     |
+-----------------+------------+------------+------------+------------+""" % (
        c.n, c.table_size, float(c.n) / c.table_size, c.probecount / c.searchcount,
        l.n, l.table_size, float(l.n) / l.table_size, l.probecount / l.searchcount,
        q.n, q.table_size, float(q.n) / q.table_size, q.probecount / q.searchcount,
        d.n, d.table_size, float(d.n) / d.table_size, d.probecount / d.searchcount,
    ))

if __name__ == '__main__':
    try:
        import sys
        quiet = sys.argv[1].startswith('q')
    except (ImportError, IndexError), e:
        quiet = False
    main(quiet=quiet)
