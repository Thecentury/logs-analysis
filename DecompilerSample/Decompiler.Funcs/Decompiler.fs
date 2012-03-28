module Decompiler.Funcs

open ICSharpCode.NRefactory.CSharp
open System.Linq

let GetFormatStringFromInvocation ( expr: InvocationExpression ) : string =
    match expr.Target with
    | :? MemberReferenceExpression as methodExpr -> 
        match methodExpr.MemberName with
        | "Format" -> 
            match methodExpr.Target with
            | :? TypeReferenceExpression as typeReference -> 
                match typeReference.Type with
                | :? PrimitiveType as primitive -> 
                    if primitive.Keyword = "string" then
                        let args = [ for i in expr.Arguments -> i ]
                        match args with
                        | a :: xs -> 
                            match a with
                            | :? PrimitiveExpression as format -> format.Value :?> string
                            | _ -> null
                        | _ -> null
                    else
                        null
                | _ -> null
            | _ -> null
        | _ -> null
    | _ -> null

let GetFormatString ( expr : Expression ) : string = 
    match expr with
    | :? PrimitiveExpression as primitive -> 
        match primitive.Value with
        | :? string as str -> str
        | _ -> null
    | :? InvocationExpression as invocation -> GetFormatStringFromInvocation (invocation)
    | _ -> null