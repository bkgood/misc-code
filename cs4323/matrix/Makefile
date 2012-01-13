CC = gcc
CFLAGS = -std=gnu99 -Wall -Wextra -pedantic -ggdb3
LDFLAGS = -lpthread

all: matrix generate dump

util.o: util.c

matrix: util.o matrix.c

generate: util.o generate.c

dump: util.o dump.c

clean:
	rm -f *.o
	rm -f matrix
	rm -f generate
	rm -f dump

.PHONY: all
