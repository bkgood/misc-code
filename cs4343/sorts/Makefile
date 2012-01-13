# William Good
# CS4343
# Programming assignment 1
# Due Oct 5, 2010

CC = gcc
CFLAGS = -g -std=gnu99 -Wall -pedantic

all: sorts

insertion.o: insertion.c
	$(CC) $(CFLAGS) -c $<

merge.o: merge.c
	$(CC) $(CFLAGS) -c $<

heap.o: heap.c
	$(CC) $(CFLAGS) -c $<

quick.o: quick.c
	$(CC) $(CFLAGS) -c $<

main.o: main.c
	$(CC) $(CFLAGS) -c $<

sorts: insertion.o merge.o heap.o quick.o main.o
	$(CC) $(CFLAGS) -o $@ $^

clean:
	rm -fv *.o sorts

.PHONY: sorts clean
