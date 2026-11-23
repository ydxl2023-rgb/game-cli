using Oathx.Core.Sdk;
using Oathx.Entity;
using System.Collections.Generic;

[Initialize]
public class EmailEntity : XEntitySerialize
{
    /// <summary>
    /// email list
    /// </summary>
    private List<Email> emails = new List<Email>();

    /// <summary>
    /// 
    /// </summary>
    public List<Email> Emails
    {
        get { return emails; }
    }

    /// <summary>
    /// current select email index
    /// </summary>
    public int selected
    { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="emailList"></param>
    public bool SetEmails(List<Email> emailList)
    {
        emails.Clear();
        selected = 0;
     
        if (emailList != null)
        {
            emails.AddRange(emailList);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Email GetEmail(int index)
    {
        if (index >= 0 && index < emails.Count)
        {
            return emails[index];
        }

        return null;
    }

    /// <summary>
    /// get current select email info
    /// </summary>
    /// <returns></returns>
    public Email GetSelectEmail() 
    {
        return GetEmail(selected); 
    }

    /// <summary>Select the row identified by the service message ID.</summary>
    public bool SelectEmail(int emailId)
    {
        int index = emails.FindIndex(item => item.id == emailId);
        if (index < 0)
            return false;

        UpdateSelect(index);

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void UpdateSelect(int index)
    {
        selected = index;
    }
}