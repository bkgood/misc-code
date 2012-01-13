#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <limits.h>

#include "util.h"

const int kDimLimit = 4;
//const int kValueLimit = INT_MAX / 4;
const int kValueLimit = 10;

void generate(FILE *f, const int rows, const int cols)
{
    write_magic(f);
    write_dims(f, rows, cols);
    int i;
    int j;
    DataType row[cols];
    for (i = 0; i < rows; ++i) {
        for (j = 0; j < cols; ++j) {
            /* row[j] = ((int) random()) % kValueLimit; */
#ifdef MATRIX_OF_DOUBLES
            row[j] = random() / ((DataType) RAND_MAX);
#else
            row[j] = ((int) random()) % kValueLimit;
#endif
        }
        write_row(f, row, cols);
    }
}
int main(int argc, char **argv)
{
    /*
     * generate matrices with dimensions m*n and n*s, write them to stdout in
     * predescribed format.
     */
    srandom((unsigned int) time(NULL));

    int a_rows;
    int a_cols_b_rows;
    int b_cols;
    switch (argc) {
    case 4:
        a_rows = atoi(argv[1]);
        a_cols_b_rows = atoi(argv[2]);
        b_cols = atoi(argv[3]);
        if (a_rows < 1 || a_cols_b_rows < 1 || b_cols < 0) {
            printf("error: all dimensions must be positive\n");
            return -1;
        }
        break;
    case 1:
        a_rows = random() % kDimLimit + 1;
        a_cols_b_rows = random() % kDimLimit + 1;
        b_cols = random() % kDimLimit + 1;
        break;
    default:
        printf("%s: a_rows a_cols_b_rows b_cols\n", argv[0]);
        return 0;
    }
    printf("generating matrix A[%d,%d] and B[%d,%d]... ", a_rows, a_cols_b_rows,
            a_cols_b_rows, b_cols);
    fflush(stdout);
    FILE *f = fopen("input.dat", "wb");
    generate(f, a_rows, a_cols_b_rows);
    generate(f, a_cols_b_rows, b_cols);
    fclose(f);
    printf("done.\n");
    return 0;
}
