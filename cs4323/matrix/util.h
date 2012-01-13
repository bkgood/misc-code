#ifndef UTIL_H
#define UTIL_H

#include <stdio.h>
#include <stdbool.h>

//#define MATRIX_OF_DOUBLES

#ifdef MATRIX_OF_DOUBLES
typedef double DataType;
#else
#define MATRIX_OF_INTS
typedef int DataType;
#endif


bool at_valid_matrix(FILE *f);
bool write_magic(FILE *f);

bool read_dims(FILE *f, int *rows, int *columns);
bool write_dims(FILE *f, int rows, int columns);

bool read_row(FILE *f, DataType *row, int columns);
bool write_row(FILE *f, const DataType *row, int columns);

#endif /* UTIL_H */
