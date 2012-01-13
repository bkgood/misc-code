(*
 * William Good
 * Nov 30, 2011
 * Due: Dec 1, 2011
 * CS3363
 * Program 4
 *)

(* Define a queue type consisting of an element tuple E of a value and another
 * queue, or NIL.
 *
 * Note that a queues are not modified on call as might be expected in a
 * procedural language like C. For instance, in the following code:
 * val q1 = enqueue(1, NIL);
 * val q2 = enqueue(2, q1);
 * nothing in q1 is modified. In C, it might be expected that q1 is a queue
 * structure modified on a call to enqueue. Because this is not the case, this
 * enables us to not implement the queue as a doubly-linked list or a circular
 * buffer, as might be done in C. Instead, the Queue value returned from
 * enqueue and deque are effectively both the only handle to that given queue
 * and simultaneously taken to be the rear (i.e. containing the most recently
 * pushed element) element.
 *
 * Effectively, the queue is being implemented as a linked list, with the first
 * element the rear and the last element the front, with a 'next element' NIL.
 *)
datatype 'a Queue = NIL | E of 'a * 'a Queue;

(* Exception to be used when a function which should not receive an empty queue
 * (e.g., dequeue) receives an empty queue
 *)
exception EmptyQueueError;

(* Returns true if a given queue is empty. *)
fun EmptyQueue(NIL) = true
  | EmptyQueue(_) = false;

(* Adds an element to the rear of the queue (or appends the element to the
 * front of our linked list). *)
fun enqueue(a, NIL) = E(a, NIL)
  | enqueue(a, n) = E(a, n);

local
  (* Returns the front value in a queue, recursing until the next element is
   * NIL and then returning the current element's data (see datatype comment
   * for why the front of the queue is the back of the linked list). *)
  fun frontval(NIL) = raise EmptyQueueError
    | frontval(E(a, NIL)) = a
    | frontval(E(_, n)) = frontval(n);

  (* Determines if a given element is the front element of a queue. Necessary
   * because we're unable to 'look ahead' when we have an element E (I can't
   * seem to do something like #2n to get the 'a Queue element of 'a Queue n
   * so it must be done in a separate function which n is passed to and then
   * deconstructed by the argument pattern matcher). *)
  fun isfront(E(a, NIL)) = true
    | isfront(_) = false;

  (* Returns a given queue without its front element (or, returns the linked
   * list representing our queue without it's last element -- essentially
   * continues to return elements until the next element is the last and then
   * stops). *)
  fun cut(NIL) = NIL
    | cut(E(a, NIL)) = NIL
    | cut(E(a, n)) = if (isfront(n)) then (E(a, NIL)) else (E(a, cut(n)));
in
  (* Returns a pair (front value, queue without front element) *)
  (* Works by first finding the front value and then generating a new queue
   * without the front element *)
  fun dequeue(NIL) = raise EmptyQueueError
    | dequeue(n) = (frontval(n), cut(n))
end;

local
  (* Prints the front of the queue followed by other elements, separated by
   * spaces (of an integer queue).
   * Must be defined as a separate function to Qprint because we must first
   * recurse to the front of the queue before beginning to print.
   *)
  fun printit(NIL) = ()
    | printit(E(a, NIL)) = print(Int.toString(a) ^ " ")
    | printit(E(a, n)) = (printit(n); print(Int.toString(a) ^ " "))
in
  (* Prints an integer queue, followed by a newline. *)
  fun Qprint(NIL) = ()
    | Qprint(e) = (printit(e); print("\n"))
end;

(* Test cases *)
val x1 = NIL;
Qprint(x1);
val y1 = enqueue(4, x1);
Qprint(y1);
val z1 = enqueue(10, y1);
Qprint(z1);
val z2 = enqueue(50, z1);
Qprint(z2);
val z3 = enqueue(5, z2);
Qprint(z3);
val z4 =  enqueue(35, z3);
Qprint(z4);
val z5 = dequeue(z4);
Qprint(#2z5);
val z6 = dequeue(#2z5);
Qprint(#2z6);

