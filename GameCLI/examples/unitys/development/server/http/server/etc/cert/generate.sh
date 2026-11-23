
echo "---- gen rsa key"
openssl genrsa -out cert_rsa.key
echo "----     tran key"
openssl pkcs8 -topk8 -in cert_rsa.key -nocrypt -out cert.key

echo "---- gen csr"
openssl req -new -key cert_rsa.key -out cert.csr

echo "---- gen cert"
openssl x509 -req -days 3650 -in cert.csr -signkey cert_rsa.key -out cert.crt

echo "---- gen pub"
openssl x509 -in cert.crt -pubkey -noout > cert.pub
echo "----     tran rsa pub"
openssl rsa -pubin -in cert.pub -RSAPublicKey_out -out cert_rsa.pub