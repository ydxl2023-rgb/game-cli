"""Author explicit Shop binding declarations using the existing Widget generator contract."""
from pathlib import Path
root=Path(__file__).resolve().parents[2]/'client/Assets/Scripts/Systems/Shop/VM'
root.mkdir(parents=True,exist_ok=True)
types={
'VMShop':[('offers','VList<VMShopOffer>','SyncScrollRect'),('gold','string','SyncText'),
 ('taskText','string','SyncText'),('feedback','string','SyncText'),('claimVisible','bool','SyncVisible'),
 ('closeClick','ButtonClick',''),('claimClick','ButtonClick','')],
'VMShopOffer':[('name','string','SyncText'),('icon','string','SyncSprite'),('price','string','SyncText'),
 ('stock','string','SyncText'),('owned','string','SyncText'),('selectClick','ButtonClick','')],
'VMShopPurchase':[('name','string','SyncText'),('description','string','SyncText'),('icon','string','SyncSprite'),
 ('quantity','string','SyncText'),('total','string','SyncText'),('stock','string','SyncText'),
 ('owned','string','SyncText'),('feedback','string','SyncText'),('minusClick','ButtonClick',''),
 ('plusClick','ButtonClick',''),('confirmClick','ButtonClick',''),('cancelClick','ButtonClick','')]}
for name,fields in types.items():
    s='using Oathx.Widget;\n\n/// <summary>Presentation bindings for '+name+'.</summary>\npublic partial class '+name+' : VM\n{\n'
    if name=='VMShopOffer':
        s+='    /// <summary>Stable original offer identity.</summary>\n    public int offerId\n    { get; set; }\n\n'
    for field,kind,sync in fields:
        s+='    /// <summary>Displays or submits '+field+'.</summary>\n'
        s+='    [Binding'+('(typeof('+sync+'))' if sync else '')+']\n    '+kind+' '+field+';\n\n'
    (root/(name+'.cs')).write_text(s+'}\n')

