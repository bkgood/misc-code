/**
 * William Good
 * CS4343
 * Programming assignment 1
 * Due Oct 5, 2010
 */

#include <stdio.h>
#include <time.h>
#include <sys/time.h>
#include <sys/resource.h>
#include <stdbool.h>

#include "sorts.h"

// these defines control the range of random numbers used in datasets
// a smaller interval can be used in development
#define MINVAL 0
#define MAXVAL RAND_MAX

#define TIMEVAL2SECS(tv) (((double) tv.tv_sec + (double) tv.tv_usec / 1000000.0))

// Convenience typedef of sort function interface
typedef void (*sort)(int *arr, int len, int *temp);

// forward declaration
static void print_arr(int *arr, int len);
static int* build_arr(int len); 
static void shuffle(int *arr, int len);
static double elapsed(double start);
static bool is_sorted(int *arr, int len);
static double time_sort(sort sortfn, int *arr, int len, int *temp);
static void print_header();
static void print_row();
static void print_algorithm(const char *algorithm);
static void print_time(double time);

// prints the given array to stdout
void print_arr(int *arr, int len) {
	for (int i = 0; i < len; ++i) {
		printf("%d ", arr[i]);
	}
	printf("\n");
}

// builds an array of random values
int* build_arr(int len) {
	int *arr = malloc(sizeof(int) * len);
	for (int *item = arr; item - arr < len; ++item) {
		*item = rrandom(MINVAL, MAXVAL);
	}
	return arr;
}

// shuffles the given array randomly
void shuffle(int *arr, int len) {
	for (int i = 0; i < len - 1; ++i) {
		exchange(arr + i, arr + rrandom(i, len));
	}
}

// returns the number of seconds elapsed since the given clock_t value
double elapsed(double starttime) {
	struct rusage endresource;
	getrusage(RUSAGE_SELF, &endresource);
	double endtime = TIMEVAL2SECS(endresource.ru_utime);
	return endtime - starttime;
}

// return true if the given data is sorted in asc order
bool is_sorted(int *arr, int len) {
	for (int i = 1; i < len; ++i) {
		if (arr[i - 1] > arr[i]) {
			return false;
		}
	}
	return true;
}

// run a sort over given data using the given function
// return the number of seconds elapsed (processor time)
double time_sort(sort sortfn, int *arr, int len, int *temp) {
	struct rusage start;
	getrusage(RUSAGE_SELF, &start);
	sortfn(arr, len, temp);
	return elapsed(TIMEVAL2SECS(start.ru_utime));
}

// prints the table header
void print_header() {
	printf(
"+---------------+-------------------------------------------------------+\n"
"|               |                       Data size                       |\n"
"+---------------+-------------+-------------+-------------+-------------+\n"
"|Algorithm      |     100     |    1000     |    10000    |    100000   |\n"
"+---------------+-------------+-------------+-------------+-------------+\n"
	);
}

// prints a row border
void print_row() {
	printf("\n"
"+---------------+-------------+-------------+-------------+-------------+\n");
}

// print the first column of a row, the name of an algorithm
void print_algorithm(const char *algorithm) {
	printf("|%15s|", algorithm);
	fflush(stdout);
}

// print the subsequent columns of a row, the time taken to sort a dataset
void print_time(double time) {
	printf(" %11f |", time);
	fflush(stdout);
}

// Main program routine
int main(int argc, char **argv) {
	int sizes[] = {
		10,
		1000,
		10000,
		100000,
	};
	char *algorithms[] = {
		"Insertion sort ",
		"Merge sort     ",
		"Heap sort      ",
		"Quick sort     ",
	};
	sort sorts[] = {
		isort,
		msort,
		hsort,
		myqsort,
	};
	const int SIZES = sizeof(sizes) / sizeof(int);
	int *datasets[SIZES];
	int *temp[SIZES];

	// seed the PRNG
	srandom(time(NULL));

	// allocate memory and fill it with random data,
	// initialize aux memory with zeros
	for (int i = 0; i < SIZES; ++i) {
		datasets[i] = build_arr(sizes[i]);
		temp[i] = calloc(sizes[i], sizeof(int));
	}

	print_header();

	const int ALGORITHMS = sizeof(algorithms) / sizeof(char*);
	// run each algorithm over each dataset size, first shuffling each dataset
	for (int algo = 0; algo < ALGORITHMS; ++algo) {
		print_algorithm(algorithms[algo]);
		for (int i = 0; i < SIZES; ++i) {
			shuffle(datasets[i], sizes[i]);
			print_time(time_sort(sorts[algo], datasets[i], sizes[i], temp[i]));
			// Uncomment next lines to test all post-sorted data for true
			// sorted-ness
//			printf("is sorted? %s\n",
//				is_sorted(datasets[i], sizes[i]) ? "yes" : "no");
		}
		print_row();
	}

	// free the dataset and aux arrays
	for (int i = 0; i < SIZES; ++i) {
		free(datasets[i]);
		free(temp[i]);
	}

	// suppress unused function warning
	(void) is_sorted;
	(void) print_arr;

	return EXIT_SUCCESS;
}
