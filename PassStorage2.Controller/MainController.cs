﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using PassStorage2.Controller.Interfaces;
using PassStorage2.Models;
using PassStorage2.Base;
using System.Threading;
using System.Windows;

namespace PassStorage2.Controller
{
    public class MainController : IController
    {
        readonly Base.DataAccessLayer.Interfaces.IStorage storage;
        readonly Base.DataCryptoLayer.Interfaces.IDecodeData decoder;
        readonly Base.DataCryptoLayer.Interfaces.IEncodeData encoder;

        protected string PasswordFirst { get; set; }
        protected string PasswordSecond { get; set; }
        protected const int ExpirationDays = 180;

        public MainController()
        {
            Logger.Instance.Debug("Creating MainController");
            storage = new Base.DataAccessLayer.FileHandler();
            decoder = new Base.DataCryptoLayer.Decoder();
            encoder = new Base.DataCryptoLayer.Encoder();
        }

        public IEnumerable<Password> GetAll()
        {
            Logger.Instance.FunctionStart();
            try
            {
                return decoder.Decode(storage.Read());
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e.Message);
                return null;
            }
            finally
            {
                Logger.Instance.FunctionEnd();
            }
        }

        public IEnumerable<Password> GetAllExpired()
        {
            Logger.Instance.FunctionStart();
            try
            {
                return decoder.Decode(storage.Read()).Where(x => (DateTime.Now - x.PassChangeTime).TotalDays >= ExpirationDays);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e.Message);
                return null;
            }
            finally
            {
                Logger.Instance.FunctionEnd();
            }
        }

        public Password Get(Guid id)
        {
            Logger.Instance.FunctionStart();
            try
            {
                return decoder.Decode(storage.Read()).ToList().FirstOrDefault(x => x.Id == id);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e.Message);
                return null;
            }
            finally
            {
                Logger.Instance.FunctionEnd();
            }
        }

        public void Delete(Guid id)
        {
            Logger.Instance.FunctionStart();
            try
            {
                var passwords = decoder.Decode(storage.Read()).ToList();
                passwords.Remove(passwords.First(x => x.Id == id));
                storage.Save(encoder.Encode(passwords));
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e);
            }
            finally
            {
                Logger.Instance.FunctionEnd();
            }
        }

        public void Save(Password pass)
        {
            Logger.Instance.FunctionStart();
            try
            {
                var passwords = decoder.Decode(storage.Read()).ToList();

                if (pass.Id == null)
                {
                    pass.SaveTime = DateTime.Now;
                    pass.PassChangeTime = DateTime.Now;
                    pass.Id = Guid.NewGuid();
                    passwords.Add(pass);
                    storage.Save(encoder.Encode(passwords));
                    return;
                }

                var passInList = passwords.First(x => x.Id == pass.Id);
                passInList.Title = pass.Title;
                passInList.Login = pass.Login;
                passInList.ViewCount = pass.ViewCount;
                passInList.SaveTime = DateTime.Now;

                if (!pass.Pass.Equals(passInList.Pass))
                {
                    passInList.Pass = pass.Pass;
                    passInList.PassChangeTime = DateTime.Now;
                }

                storage.Save(encoder.Encode(passwords));
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e);
            }
            finally
            {
                Logger.Instance.FunctionEnd();
            }
        }

        public void Backup()
        {
            throw new NotImplementedException();
        }

        public void BackupDecoded()
        {
            throw new NotImplementedException();
        }

        public bool SetPasswords(string primary, string secondary)
        {
            Logger.Instance.FunctionStart();
            try
            {
                using (var protection = new Base.DataCryptoLayer.EntryProtection(primary, secondary))
                {
                    protection.Validate();

                    if (!protection.IsAllowed)
                    {
                        Logger.Instance.Error("Passwords are incorrect");
                        return false;
                    }

                    PasswordFirst = primary;
                    PasswordSecond = secondary;
                    Logger.Instance.Debug("Passwords ok");
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e);
                return false;
            }
            finally
            {
                Logger.Instance.FunctionEnd();
            }
        }

        public void UpdateViewCount(Guid id, int counter)
        {
            Logger.Instance.FunctionStart();
            try
            {
                var password = Get(id);
                password.ViewCount = counter;
                Save(password);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e);
                throw;
            }
        }

        public void IncrementViewCount(Guid id)
        {
            Logger.Instance.FunctionStart();
            try
            {
                var password = Get(id);
                password.ViewCount++;
                Save(password);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e);
                throw;
            }
        }

        public IEnumerable<PasswordExt> GetAllExtended()
        {
            Logger.Instance.FunctionStart();
            try
            {
                var passExtList = new List<PasswordExt>();
                var passList = GetAll();

                foreach (var pass in passList)
                {
                    var passExt = new PasswordExt
                    {
                        Id = pass.Id,
                        Login = pass.Login,
                        Title = pass.Title,
                        Pass = pass.Pass,
                        PassChangeTime = pass.PassChangeTime,
                        SaveTime = pass.SaveTime,
                        ViewCount = pass.ViewCount,
                        IsValid = (DateTime.Now - pass.PassChangeTime).TotalDays <= ExpirationDays,
                    };

                    passExt.WarningIconState = passExt.IsValid ? Visibility.Hidden : Visibility.Hidden;
                    passExtList.Add(passExt);
                }

                return passExtList;
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e);
                return null;
            }
        }
    }
}
