namespace Browser.Types

open Fable.Core

type MutationRecord =
    /// The nodes added by a mutation. Will be an empty NodeList if no nodes were added
    abstract addedNodes: NodeList
    /// The name of the changed attribute as a string, or null.
    abstract attributeName: string
    /// The namespace of the changed attribute as a string, or null.
    abstract atttributeNamespace: string
    /// The next sibling of the added or removed nodes, or null
    abstract nextSibling: Node
    /// The value depends on the MutationRecord.type:
    /// - For attributes, it is the value of the changed attribute before the change.
    /// - For characterData, it is the data of the changed node before the change.
    /// - For childList, it is null.
    abstract oldValue: obj
    /// The previous sibling of the added or removed nodes, or null
    abstract previousSibling: Node
    /// The nodes removed by a mutation. Will be an empty NodeList if no nodes were removed
    abstract removedNodes: NodeList
    ///The node the mutation affected, depending on the MutationRecord.type.
    /// 
    /// - For attributes, it is the element whose attribute changed.
    /// - For characterData, it is the CharacterData node.
    /// - For childList, it is the node whose children changed.
    abstract target : Node
    /// A string representing the type of mutation: attributes if the mutation was an attribute mutation, characterData if it was a mutation to a CharacterData node, and childList if it was a mutation to the tree of nodes.
    abstract ``type``: string

type MutationObserveOptions = 
    /// Set to true to extend monitoring to the entire subtree of nodes rooted at target. All of the other properties are then extended to all of the nodes in the subtree instead of applying solely to the target node. The default value is false. 
    abstract subtree : bool with get,set
    /// Set to true to monitor the target node (and, if subtree is true, its descendants) for the addition of new child nodes or removal of existing child nodes. The default value is false. 
    abstract childList : bool with get,set
    /// Set to true to watch for changes to the value of attributes on the node or nodes being monitored. The default value is true if either of attributeFilter or attributeOldValue is specified, otherwise the default value is false.
    abstract attributes : bool with get,set
    /// An array of specific attribute names to be monitored. If this property isn't included, changes to all attributes cause mutation notifications. 
    abstract attributeFilter : bool with get,set
    /// Set to true to record the previous value of any attribute that changes when monitoring the node or nodes for attribute changes; See Monitoring attribute values for details on watching for attribute changes and value recording. The default value is false. 
    abstract attributeOldValue : bool with get,set
    /// Set to true to monitor the specified target node (and, if subtree is true, its descendants) for changes to the character data contained within the node or nodes. The default value is true if characterDataOldValue is specified, otherwise the default value is false. 
    abstract characterData : bool with get,set
    /// Set to true to record the previous value of a node's text whenever the text changes on nodes being monitored. The default value is false.
    abstract characterDataOldValue : bool with get,set
    [<Emit("{ }")>] abstract member Create: unit -> MutationObserveOptions

type [<AllowNullLiteral; Global>] MutationObserverType =
    /// Stops the MutationObserver instance from receiving further notifications until and unless observe() is called again.
    abstract disconnect: unit -> unit
    /// Configures the MutationObserver to begin receiving notifications through its callback function when DOM changes matching the given options occur.
    abstract observe: Node * MutationObserveOptions -> unit
    /// Removes all pending notifications from the MutationObserver's notification queue and returns them in a new Array of MutationRecord objects.
    abstract takeRecords: Node -> unit

type MutationObserverCallback = MutationRecord[] -> MutationObserverType -> unit

type [<Global>] MutationObserverCtor =
    [<Emit("new $0($1...)")>] abstract Create: url: MutationObserverCallback -> MutationObserverType
    [<Emit("{ }")>] abstract CreateOptions: unit -> MutationObserveOptions

namespace Browser

open Fable.Core
open Browser.Types

[<AutoOpen>]
module MutationObserver =
    let [<Global>] MutationObserver: MutationObserverCtor = jsNative
