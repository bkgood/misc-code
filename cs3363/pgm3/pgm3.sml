(* William Good
 * CS3363
 * Programming assignment 3
 * Nov 17, 2011
 *)

local
  (* Inserts a real x into a sorted list with the ordering given by cmp. *)
  fun ins(cmp, x, []) = [x]
    | ins(cmp, x, xs as h::t) = if (cmp(x, h)) then x::xs else h::(ins(cmp, x,t))
in
  (* Sorts a list by repeatedly inserting the first element of the provided list
  * into a new, sorted list. At each iteration, the size of the list decreases
  * by one, until a base empty case is reached. A provided function is used to
  * determine ordering. *)
  fun PolySort(cmp, []) = []
    | PolySort(cmp, x::xs) = ins(cmp, x, PolySort(cmp, xs))
end;


(* Standard ML doesn't support generic functions so all these must be spelled
* out... *)
fun intlt(a: int, b) = a < b;
fun reallt(a: real, b) = a < b;
fun charlt(a: char, b) = a < b;
fun stringlt(a: string, b) = a < b;

fun intgt(a: int, b) = a > b;
fun realgt(a: real, b) = a > b;
fun chargt(a: char, b) = a > b;
fun stringgt(a: string, b) = a > b;

PolySort(intlt, []);
PolySort(intgt, []);
PolySort(intlt, [1]);
PolySort(intgt, [1]);
PolySort(intlt, [7,3,4,8,5]);
PolySort(intgt, [7,3,4,8,5]);
PolySort(reallt, [10.1]);
PolySort(realgt, [10.1]);
PolySort(reallt, [3.5,7.9,1.5,10.1,5.3,8.1,6.1,0.1,2.5,4.2,1.5,7.8]);
PolySort(realgt, [3.5,7.9,1.5,10.1,5.3,8.1,6.1,0.1,2.5,4.2,1.5,7.8]);
PolySort(charlt, [#"c", #"z", #"d", #"v"]);
PolySort(chargt, [#"c", #"z", #"d", #"v"]);
PolySort(stringlt, ["CS", "3363", "Programming", "Languages"]);
PolySort(stringgt, ["CS", "3363", "Programming", "Languages"]);
