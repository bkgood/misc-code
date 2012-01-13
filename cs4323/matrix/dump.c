#include <stdio.h>
#include <assert.h>
#include <stdlib.h>
#include <string.h>

#include "util.h"

/**
 * Pretty prints every matrices in a file until error or eof.
 */
void pretty_print(FILE *f)
{
    while (at_valid_matrix(f)) {
        int rows;
        int cols;
        assert(read_dims(f, &rows, &cols));
        printf("found %dx%d matrix\n", rows, cols);
        DataType *row = calloc(cols, sizeof(*row));
        int i;
        for (i = 0; i < rows; i++) {
            memset(row, 0, sizeof(*row) * cols);
            assert(read_row(f, row, cols));
            int j;
            for (j = 0; j < cols; j++) {
#ifdef MATRIX_OF_DOUBLES
                printf("%f ", row[j]);
#else
                printf("%d ", row[j]);
#endif
            }
            putchar('\n');
        }
        free(row);
    }
}

void dumpfile(const char *path)
{
    FILE *f = fopen(path, "rb");
    if (!f) {
        printf("failed to open file %s\n", path);
        return;
    }
    printf("%s\n", path);
    pretty_print(f);
    fclose(f);
}


int main(int argc, char **argv)
{
    if (argc > 1) {
        int i;
        for (i = 1; i < argc; i++) {
            dumpfile(argv[i]);
        }
    } else {
        dumpfile("input.dat");
    }
    return 0;
}
