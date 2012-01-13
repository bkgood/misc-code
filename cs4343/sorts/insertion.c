/**
 * William Good
 * CS4343
 * Programming assignment 1
 * Due Oct 5, 2010
 */

#include "sorts.h"

void isort(int *arr, int len, int *temp) {
	int *end = arr + len;
	for (int *sorted = arr + 1; sorted < end; ++sorted) {
		// sorted is the boundary between sorted and unsorted values
		// iterate until the boundary moves to the end of the array
		int save = *sorted;
		int *comp;
		for (comp = sorted - 1; comp >= arr; --comp) {
			// move down through the sorted values, moving each value up one
			// once the sorted value is lte the boundary (unsorted) value,
			// don't move it up, break out of the loop and...
			if (*comp <= save) {
				break;
			} else {
				*(comp + 1) = *comp;
			}
		}
		// copy the boundary value just after it
		*(comp + 1) = save;
	}
}
