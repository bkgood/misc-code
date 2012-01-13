// William Good
// CS4323
// Assignment 3

#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#include <string.h>
#include <assert.h>
#include <errno.h>
#include <unistd.h>

#include "util.h"

const char *kInputFile = "input.dat";
const char *kOutputFile = "output.dat";

/** TODO: add floating point toggle and width options
 * Will need to change pointers to void pointers and then cast as necessary.
 * Won't have to recompile to move between float and int matrices
 */
typedef struct {
    int rows;
    int columns;
    DataType **data;
    DataType *column_tmp;
} Matrix;

typedef struct ThreadData_ {
    const Matrix *A;
    const Matrix *B;
    Matrix *C;
    int *rows; // rows thread is to compute, -1 terminated
    int thread_id;
} ThreadData;

typedef Matrix* (*MultiplierMethod)(const Matrix *A, const Matrix *B, int argc, char **argv);

typedef struct {
    const char *name;
    const char *desc;
    MultiplierMethod meth;
} Multiplier;

Matrix* create_matrix(FILE *in);
Matrix* new_matrix(const int rows, const int columns);
void print_matrix(const Matrix *m);
void write_matrix(const Matrix *m, FILE *f);
void free_matrix(Matrix **m);
static int* read_row_old(const char *line, int n);
DataType calculate_entry(const Matrix *A, const Matrix *B, int i, int j);
void calculate_row(const Matrix *A, const Matrix *B, Matrix *C, int row);
DataType dot_product(const DataType *a, const DataType *b, int n);
void* thread_proc(void *arg);
Matrix* calc_threads(const Matrix *A, const Matrix *B, int argc, char **argv);
Matrix* calc_nothreads(const Matrix *A, const Matrix *B, int argc, char **argv);

/**
 * Creates a matrix from an input stream. Returns data as an array of rows.
 * Caller responsible for freeing returned structure.
 */
Matrix* create_matrix(FILE *in) {
    Matrix *m = NULL;
    int rows = 0;
    int columns = 0;

    if (!at_valid_matrix(in)) goto MatrixMalformedError;
    if (!read_dims(in, &rows, &columns)) {
        goto MatrixDimError;
    }
    m = new_matrix(rows, columns);
    int i = 0;
    for (i = 0; i < m->rows; ++i) {
        if (!read_row(in, m->data[i], m->columns)) {
            goto MatrixEntryError;
        }
    }
    return m;
    // evil gotos for exception handling
MatrixMalformedError:
    printf("The input.dat file appears corrupt, are you sure it's the proper format?\n");
    goto FreeAndReturn;
MatrixDimError:
    printf("Matrix dimension data corrupt\n");
    goto FreeAndReturn;
MatrixEntryError:
    printf("Matrix data corrupt\n");
    goto FreeAndReturn;
FreeAndReturn:
    free_matrix(&m);
    return NULL;
}

/**
 * Creates a new matrix.
 */
Matrix* new_matrix(const int rows, const int columns)
{
    Matrix *m = calloc(1, sizeof(*m));
    m->rows = rows;
    m->columns = columns;
    m->column_tmp = calloc(m->rows, sizeof(*m->column_tmp));
    m->data = calloc(m->rows, sizeof(m->data));
    for (int i = 0; i < m->rows; ++i) {
        m->data[i] = calloc(m->columns, sizeof(*m->data[i]));
    }
    return m;
}

/**
 * Writes a matrix to a generic stream
 */
void write_matrix(const Matrix *m, FILE *f) {
    write_magic(f);
    write_dims(f, m->rows, m->columns);
    int i;
    for (i = 0; i < m->rows; ++i) {
        write_row(f, m->data[i], m->columns);
    }
}

/**
 * Frees a matrix in the format provided by create_matrix
 */
void free_matrix(Matrix **m) {
    Matrix *A = *m;
    if (A) {
        if (A->data) {
            int i;
            for (i = 0; i < A->rows; ++i) {
                free(A->data[i]);
            }
            free(A->data);
        }
        free(A->column_tmp);
        free(A);
    }
    *m = NULL;
}

/**
 * Reads a row into a matrix, returning an int[n] on success or null on
 * failure. Returned memory is owned by the caller.
 */
static int* read_row_old(const char *line, int n) {
    /* Strategy here: convert each space in the line to a null
     * byte, and then call sscanf on each substring (atoi doesn't give us the
     * same rigor in error checking). this should cause us to
     * end up with n ints, which we can then put in an int[]
     */
    char *buff = strdup(line);
    ssize_t llen = strlen(buff);
    int i;
    for (i = 0; i < llen; ++i) {
        switch (buff[i]) {
        case ' ':
        case '\t':
            // are there other cases to worry about here?
            buff[i] = '\0';
            break;
        }
    }
    int *data = calloc(n, sizeof(int));
    char *cur = buff;
    i = 0;
    while (cur < buff + llen && i < n) {
        if (sscanf(cur, "%d", &data[i++]) != 1) {
            i = 0;
            break;
        }
        do { cur++; } while (*cur != '\0'); cur++;
    }
    if (i != n) {
        // means we didn't get enough integers
        free(data);
        data = NULL;
    }
    free(buff);
    buff = NULL;
    return data;
}

/**
 * Returns the integer at C(i,j) where C=AB. Works out to be the inner
 * product between A(i,) and B(,j)
 */
DataType calculate_entry(const Matrix *A, const Matrix *B, int i, int j)
{
    // C(i,j) is row i of A * column j of B
    assert(A->columns == B->rows);
    DataType *column = B->column_tmp;
    int k;
    for (k = 0; k < B->rows; k++) {
        column[k] = B->data[k][j];
    }
    DataType entry = dot_product(A->data[i], column, A->columns);
    return entry;
}

/**
 * Calculates C(i,) where C=AB.
 */
void calculate_row(const Matrix *A, const Matrix *B, Matrix *C, int i) {
    DataType *row = C->data[i];
    int j;
    for (j = 0; j < B->columns; ++j) {
        row[j] = calculate_entry(A, B, i, j);
    }
}

/**
 * Takes a dot product of two "vectors" (int[]) of length n (dot product
 * is undefined where len(a) != len(b))
 */
DataType dot_product(const DataType *a, const DataType *b, int n) {
    DataType prod = 0.0;
    int i = 0;
    for (i = 0; i < n; ++i) {
        prod += a[i] * b[i];
    }
    return prod;
}

/**
 * A calculating thread's process.
 */
void* thread_proc(void *arg) {
    ThreadData *data = (ThreadData*) arg;
    int *rows = data->rows;
    int *rows_start = rows;
    int rows_count = 0;
    while (*rows > -1 && rows[rows_count++ + 1] > -1);
    while (*rows > -1) {
        calculate_row(data->A, data->B, data->C, *rows);
        rows++;
        if (rows_count >= 100 && (rows - rows_start) % (rows_count / 100) == 0) {
            /* due to multiple threads writing the %f\r sequence at once,
             * is a little messy, but tolerable
             */
            printf(" %td%%\r", (rows - rows_start) / (rows_count / 100));
            fflush(stdout);
        }
    }
    if (rows_count >= 100) putchar('\n');
    fflush(stdout);
    return data;
}

/**
 * Calculates a product of two matrices, using threads to calculate each row
 * of the product.
 */
Matrix* calc_threads(const Matrix *A, const Matrix *B, int argc, char **argv) {
    Matrix *C = new_matrix(A->rows, B->columns);

    int procs = (argc > 0) ? atoi(argv[0]) : (int) sysconf(_SC_NPROCESSORS_ONLN);
    if (procs > C->rows) procs = C->rows;
    printf("initializing %d threads\n", procs);
    pthread_t *threads = calloc(procs, sizeof(pthread_t));
    ThreadData *datas = calloc(procs, sizeof(ThreadData));

    int proc = 0;
    const int rows_per_proc = C->rows / procs;
    int row = 0;
    while (row < C->rows) {
        datas[proc].A = A;
        datas[proc].B = B;
        datas[proc].C = C;
        int row_to_calc_count = (proc == procs - 1) ? C->rows - row : rows_per_proc;
        // +1 on these next two lines to account for the -1 terminator
        datas[proc].rows = calloc(row_to_calc_count + 1, sizeof(*datas[proc].rows));
        memset(datas[proc].rows, -1, sizeof(*datas[proc].rows) * (row_to_calc_count + 1));
        int i = 0;
        while (i < row_to_calc_count && row < C->rows) {
            datas[proc].rows[i++] = row++;
        }
        assert(!pthread_create(&threads[proc], NULL, thread_proc, &datas[proc]));
        proc++;
    }
    int i;
    for (i = 0; i < procs; ++i) {
        assert(!pthread_join(threads[i], NULL));
    }
    free(threads);
    while (proc > 0) {
        free(datas[proc-- - 1].rows);
    }
    free(datas);
    return C;
}

/**
 * Calculates the product of two matrices, without parallelization (used for
 * testing more than anything)
 */
Matrix* calc_nothreads(const Matrix *A, const Matrix *B, int argc, char **argv) {
    (void) argc;
    (void) argv;
    Matrix *C = new_matrix(A->rows, B->columns);
    int r;
    for (r = 0; r < A->rows; ++r) {
        calculate_row(A, B, C, r);
        if (A->rows >= 100 && r % (A->rows / 100) == 0) {
            printf(" %d%%\r", r / (A->rows / 100));
            fflush(stdout);
        }
    }
    if (A->rows >= 100) putchar('\n');
    fflush(stdout);
    return C;
}

const Multiplier multipliers[] = {
    {
        "serial", "calculates matrix product in a single thread", calc_nothreads
    },
    {
        "threads", "calculates matrix product in multiple threads; takes a "
            "number of threads or uses system cpu count", calc_threads
    },
    {
        NULL, NULL, NULL
    },
};

int main(int argc, char **argv) {
    Matrix *A, *B;
    MultiplierMethod mult_method = NULL;
    const Multiplier *m = &multipliers[0];
    if (argc < 2) goto usage;
    while (m->name) {
        if (!strcmp(argv[1], m->name)) {
            mult_method = m->meth;
            break;
        }
        ++m;
    }
    if (!m) {
        goto usage;
    }

    FILE *input = fopen(kInputFile, "rb");
    if (!input) {
        printf("unable to open %s\n", kInputFile);
        return 1;
    }
    if (!(A = create_matrix(input))) {
        printf("create matrix A failed\n");
        fclose(input);
        return 1;
    }
    if (!(B = create_matrix(input))) {
        printf("create matrix B failed\n");
        fclose(input);
        return 1;
    }
    fclose(input);

    Matrix *C = mult_method(A, B, argc - 2, argv + 2);
    FILE *out = fopen(kOutputFile, "wb");
    write_matrix(C, out);
    fclose(out);
    free_matrix(&C);
    free_matrix(&A);
    free_matrix(&B);
    return 0;
usage:
    printf("%s: method ...\n" "available methods:\n", argv[0]);
    m = &multipliers[0];
    while (m->name) {
        printf(" %s: %s\n", m->name, m->desc);
        ++m;
    }
    return 1;
    (void) read_row_old;
}

