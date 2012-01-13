#include <string.h>
#include <stdlib.h>

#include "util.h"

/**
 * we use 8 bytes of 'magic' to make sure what we're reading isn't total
 * garbage. The first 6 bytes are the string "MATRIX" (no terminator),
 * byte 7 is a format byte, and byte 8 is currently unused
 */
const char *kMagic = "MATRIX";
const int kMagicLength = 8;
#ifdef MATRIX_OF_DOUBLES
const char format = 1;
#else
const char format = 0;
#endif

bool at_valid_matrix(FILE *f)
{
    char *hope_its_magic = alloca(kMagicLength * sizeof(*hope_its_magic));
    memset(hope_its_magic, 0, kMagicLength * sizeof(*hope_its_magic));
    fread(hope_its_magic, sizeof(char), kMagicLength, f);
    return !ferror(f) && !feof(f)
        && !strncmp(hope_its_magic, kMagic, strlen(kMagic))
        && hope_its_magic[6] == format;
}

bool write_magic(FILE *f)
{
    char *magic = alloca(kMagicLength * sizeof(*magic));
    memset(magic, 0, kMagicLength * sizeof(*magic));
    strncpy(magic, kMagic, strlen(kMagic));
    magic[6] = format;
    return fwrite(magic, sizeof(char), kMagicLength, f) == (unsigned int) kMagicLength;
}

bool read_dims(FILE *f, int *rows, int *columns)
{
    fread(rows, sizeof(int), 1, f);
    if (ferror(f) || feof(f)) return false;
    fread(columns, sizeof(int), 1, f);
    if (ferror(f) || feof(f)) return false;
    return true;
}

bool write_dims(FILE *f, int rows, int columns)
{
    fwrite(&rows, sizeof(int), 1, f);
    if (ferror(f) || feof(f)) return false;
    fwrite(&columns, sizeof(int), 1, f);
    if (ferror(f) || feof(f)) return false;
    return true;
}

bool read_row(FILE *f, DataType *row, int columns)
{
    return fread(row, sizeof(*row), columns, f) == (unsigned int) columns && !ferror(f);
}

bool write_row(FILE *f, const DataType *row, int columns)
{
    return fwrite(row, sizeof(*row), columns, f) == (unsigned int) columns && !ferror(f);
}
