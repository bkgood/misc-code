(* William Good
 * CS3363
 * Programming assignment 2
 * Nov 15, 2011
 *)

local
  (* Inserts a real x into a sorted list. *)
  fun ins(x, []) = [x]
    | ins(x: real, xs as h::t) = if (x < h) then x::xs else h::(ins(x,t))
in
  (* Sorts a list by repeatedly inserting the first element of the provided list
  * into a new, sorted list. At each iteration, the size of the list decreases
  * by one, until a base empty case is reached. *)
  fun insort([]) = [] | insort(x::xs) = ins(x, insort(xs))
end;

insort([]);
insort([10.10]);
insort([2.5,1.7]);
insort([1.2,2.3,2.3,4.0]);
val L = [3.5,7.9,1.5,10.1,5.3,8.1,6.1,0.1,2.5,4.2,1.5,7.8];
insort(L);
